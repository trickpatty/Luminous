using FluentValidation;
using Luminous.Application.Common.Interfaces;
using Luminous.Application.DTOs;
using Luminous.Domain.Entities;
using Luminous.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Luminous.Application.Features.Auth.Commands;

/// <summary>
/// Command to verify an OTP and authenticate the user.
/// </summary>
public sealed record VerifyOtpCommand : IRequest<OtpVerifyResultDto>
{
    /// <summary>
    /// The email address the OTP was sent to.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// The OTP code to verify.
    /// </summary>
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// The client IP address.
    /// </summary>
    public string? IpAddress { get; init; }

    /// <summary>
    /// The client user agent.
    /// </summary>
    public string? UserAgent { get; init; }
}

/// <summary>
/// Validator for VerifyOtpCommand.
/// </summary>
public sealed class VerifyOtpCommandValidator : AbstractValidator<VerifyOtpCommand>
{
    public VerifyOtpCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("OTP code is required.")
            .Length(6).WithMessage("OTP code must be 6 digits.")
            .Matches(@"^\d{6}$").WithMessage("OTP code must contain only digits.");
    }
}

/// <summary>
/// Handler for VerifyOtpCommand.
/// </summary>
public sealed class VerifyOtpCommandHandler : IRequestHandler<VerifyOtpCommand, OtpVerifyResultDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly ILogger<VerifyOtpCommandHandler> _logger;

    public VerifyOtpCommandHandler(
        IUnitOfWork unitOfWork,
        ITokenService tokenService,
        ILogger<VerifyOtpCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<OtpVerifyResultDto> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.ToLowerInvariant();

        // Get the latest active OTP for this email
        var otpToken = await _unitOfWork.OtpTokens.GetLatestActiveByEmailAsync(
            normalizedEmail,
            cancellationToken);

        if (otpToken == null)
        {
            _logger.LogWarning("No active OTP found for email {Email}", normalizedEmail);
            return new OtpVerifyResultDto
            {
                Success = false,
                Error = "Invalid or expired OTP. Please request a new one.",
                RemainingAttempts = 0
            };
        }

        // Verify the code
        if (!otpToken.VerifyCode(request.Code))
        {
            await _unitOfWork.OtpTokens.UpdateAsync(otpToken, cancellationToken);

            var remainingAttempts = otpToken.MaxAttempts - otpToken.Attempts;
            _logger.LogWarning(
                "Invalid OTP attempt for email {Email}. Remaining attempts: {Remaining}",
                normalizedEmail,
                remainingAttempts);

            if (remainingAttempts <= 0)
            {
                return new OtpVerifyResultDto
                {
                    Success = false,
                    Error = "Too many invalid attempts. Please request a new OTP.",
                    RemainingAttempts = 0
                };
            }

            return new OtpVerifyResultDto
            {
                Success = false,
                Error = "Invalid OTP code.",
                RemainingAttempts = remainingAttempts
            };
        }

        // OTP is valid - mark as used
        await _unitOfWork.OtpTokens.UpdateAsync(otpToken, cancellationToken);

        // Get the user
        var user = await _unitOfWork.Users.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (user == null)
        {
            _logger.LogError("User not found for verified OTP email {Email}", normalizedEmail);
            return new OtpVerifyResultDto
            {
                Success = false,
                Error = "User not found. Please register first.",
                RemainingAttempts = 0
            };
        }

        // Mark email as verified if not already
        if (!user.EmailVerified)
        {
            user.EmailVerified = true;
            await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
        }

        // Record login
        user.RecordLogin();
        await _unitOfWork.Users.UpdateAsync(user, cancellationToken);

        // Create refresh token
        var (refreshToken, rawRefreshToken) = RefreshToken.Create(
            user.Id,
            user.FamilyId,
            TimeSpan.FromDays(7),
            request.IpAddress,
            request.UserAgent);

        await _unitOfWork.RefreshTokens.AddAsync(refreshToken, cancellationToken);

        // Generate access token
        var authResult = _tokenService.GenerateToken(user);

        // Replace the auto-generated refresh token with our persisted one
        authResult = authResult with { RefreshToken = rawRefreshToken };

        _logger.LogInformation("User {UserId} authenticated via OTP", user.Id);

        return new OtpVerifyResultDto
        {
            Success = true,
            Auth = authResult,
            RemainingAttempts = 0
        };
    }
}
