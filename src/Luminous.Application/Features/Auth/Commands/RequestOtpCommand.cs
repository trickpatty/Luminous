using FluentValidation;
using Luminous.Application.Common.Interfaces;
using Luminous.Application.DTOs;
using Luminous.Domain.Entities;
using Luminous.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Luminous.Application.Features.Auth.Commands;

/// <summary>
/// Command to request an OTP for email-based authentication.
/// </summary>
public sealed record RequestOtpCommand : IRequest<OtpRequestResultDto>
{
    /// <summary>
    /// The email address to send the OTP to.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// The purpose of the OTP (login, registration).
    /// </summary>
    public string Purpose { get; init; } = "login";

    /// <summary>
    /// The client IP address (for rate limiting).
    /// </summary>
    public string? IpAddress { get; init; }

    /// <summary>
    /// The client user agent.
    /// </summary>
    public string? UserAgent { get; init; }
}

/// <summary>
/// Validator for RequestOtpCommand.
/// </summary>
public sealed class RequestOtpCommandValidator : AbstractValidator<RequestOtpCommand>
{
    public RequestOtpCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters.");

        RuleFor(x => x.Purpose)
            .NotEmpty().WithMessage("Purpose is required.")
            .Must(x => x is "login" or "registration").WithMessage("Purpose must be 'login' or 'registration'.");
    }
}

/// <summary>
/// Handler for RequestOtpCommand.
/// </summary>
public sealed class RequestOtpCommandHandler : IRequestHandler<RequestOtpCommand, OtpRequestResultDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly ILogger<RequestOtpCommandHandler> _logger;

    // Rate limiting constants
    private const int MaxRequestsPerHour = 5;
    private const int MinSecondsBetweenRequests = 60;

    public RequestOtpCommandHandler(
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        ILogger<RequestOtpCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<OtpRequestResultDto> Handle(RequestOtpCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.ToLowerInvariant();

        // Check rate limiting
        var recentRequests = await _unitOfWork.OtpTokens.CountRecentRequestsAsync(
            normalizedEmail,
            TimeSpan.FromHours(1),
            cancellationToken);

        if (recentRequests >= MaxRequestsPerHour)
        {
            _logger.LogWarning("OTP rate limit exceeded for email {Email}", normalizedEmail);
            return new OtpRequestResultDto
            {
                Success = false,
                Message = "Too many OTP requests. Please try again later.",
                MaskedEmail = MaskEmail(normalizedEmail),
                RetryAfterSeconds = 3600 // 1 hour
            };
        }

        // Check if there's a recent active OTP (prevent spam)
        var latestOtp = await _unitOfWork.OtpTokens.GetLatestActiveByEmailAsync(
            normalizedEmail,
            cancellationToken);

        if (latestOtp != null)
        {
            var secondsSinceCreation = (DateTime.UtcNow - latestOtp.CreatedAt).TotalSeconds;
            if (secondsSinceCreation < MinSecondsBetweenRequests)
            {
                var retryAfter = (int)(MinSecondsBetweenRequests - secondsSinceCreation);
                return new OtpRequestResultDto
                {
                    Success = false,
                    Message = "Please wait before requesting a new OTP.",
                    MaskedEmail = MaskEmail(normalizedEmail),
                    ExpiresAt = latestOtp.ExpiresAt,
                    RetryAfterSeconds = retryAfter
                };
            }
        }

        // For login, check if user exists
        User? user = null;
        if (request.Purpose == "login")
        {
            user = await _unitOfWork.Users.GetByEmailAsync(normalizedEmail, cancellationToken);
            if (user == null)
            {
                // For security, don't reveal if the email exists
                _logger.LogInformation("OTP requested for non-existent email {Email}", normalizedEmail);

                // Still return success to prevent email enumeration
                return new OtpRequestResultDto
                {
                    Success = true,
                    Message = "If this email is registered, you will receive an OTP shortly.",
                    MaskedEmail = MaskEmail(normalizedEmail),
                    ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                    RetryAfterSeconds = MinSecondsBetweenRequests
                };
            }
        }

        // Invalidate any existing active OTPs
        await _unitOfWork.OtpTokens.InvalidateAllForEmailAsync(normalizedEmail, cancellationToken);

        // Create new OTP
        var (otpToken, code) = OtpToken.Create(
            normalizedEmail,
            TimeSpan.FromMinutes(10),
            request.Purpose,
            user?.Id,
            user?.FamilyId,
            request.IpAddress,
            request.UserAgent);

        await _unitOfWork.OtpTokens.AddAsync(otpToken, cancellationToken);

        // Send OTP via email
        try
        {
            await _emailService.SendOtpAsync(normalizedEmail, code, cancellationToken);
            _logger.LogInformation("OTP sent to {Email} for {Purpose}", normalizedEmail, request.Purpose);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send OTP to {Email}", normalizedEmail);
            // Still return success to prevent information leakage
        }

        return new OtpRequestResultDto
        {
            Success = true,
            Message = "If this email is registered, you will receive an OTP shortly.",
            MaskedEmail = MaskEmail(normalizedEmail),
            ExpiresAt = otpToken.ExpiresAt,
            RetryAfterSeconds = MinSecondsBetweenRequests
        };
    }

    private static string MaskEmail(string email)
    {
        var atIndex = email.IndexOf('@');
        if (atIndex <= 1) return email;

        var prefix = email[..atIndex];
        var domain = email[atIndex..];

        if (prefix.Length <= 2)
        {
            return $"{prefix[0]}*{domain}";
        }

        var visibleChars = Math.Min(2, prefix.Length / 3);
        var masked = $"{prefix[..visibleChars]}{"*".PadRight(prefix.Length - visibleChars, '*')}{domain}";
        return masked;
    }
}
