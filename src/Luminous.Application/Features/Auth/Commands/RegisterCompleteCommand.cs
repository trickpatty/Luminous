using FluentValidation;
using Luminous.Application.Common.Exceptions;
using Luminous.Application.Common.Interfaces;
using Luminous.Application.DTOs;
using Luminous.Application.Features.Auth.Models;
using Luminous.Domain.Entities;
using Luminous.Domain.Enums;
using Luminous.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Luminous.Application.Features.Auth.Commands;

/// <summary>
/// Command to complete registration by verifying the OTP and creating the account.
/// This is step 2 of the secure registration flow.
/// Email is retrieved from the session to prevent identity impersonation attacks.
/// </summary>
public sealed record RegisterCompleteCommand : IRequest<RegisterCompleteResultDto>
{
    /// <summary>
    /// The session ID from the registration start.
    /// </summary>
    public string SessionId { get; init; } = string.Empty;

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
/// Validator for RegisterCompleteCommand.
/// </summary>
public sealed class RegisterCompleteCommandValidator : AbstractValidator<RegisterCompleteCommand>
{
    public RegisterCompleteCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("Session ID is required.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Verification code is required.")
            .Length(6).WithMessage("Verification code must be 6 digits.")
            .Matches(@"^\d{6}$").WithMessage("Verification code must contain only digits.");
    }
}

/// <summary>
/// Handler for RegisterCompleteCommand.
/// </summary>
public sealed class RegisterCompleteCommandHandler : IRequestHandler<RegisterCompleteCommand, RegisterCompleteResultDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IDistributedCache _cache;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<RegisterCompleteCommandHandler> _logger;

    public RegisterCompleteCommandHandler(
        IUnitOfWork unitOfWork,
        ITokenService tokenService,
        IDistributedCache cache,
        IDateTimeService dateTimeService,
        ILogger<RegisterCompleteCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _cache = cache;
        _dateTimeService = dateTimeService;
        _logger = logger;
    }

    public async Task<RegisterCompleteResultDto> Handle(RegisterCompleteCommand request, CancellationToken cancellationToken)
    {
        // Retrieve registration session data - this is our trusted source for the email
        var sessionJson = await _cache.GetStringAsync(
            $"registration:{request.SessionId}",
            cancellationToken);

        if (string.IsNullOrEmpty(sessionJson))
        {
            _logger.LogWarning("Registration session not found: {SessionId}", request.SessionId);
            return new RegisterCompleteResultDto
            {
                Success = false,
                Error = "Registration session expired. Please start over.",
                RemainingAttempts = 0
            };
        }

        var sessionData = JsonSerializer.Deserialize<RegistrationSessionData>(sessionJson);
        if (sessionData == null)
        {
            _logger.LogWarning("Failed to deserialize registration session: {SessionId}", request.SessionId);
            return new RegisterCompleteResultDto
            {
                Success = false,
                Error = "Invalid registration session. Please start over.",
                RemainingAttempts = 0
            };
        }

        // Use the email from the session (trusted) - not from the request
        var normalizedEmail = sessionData.Email;

        // Get the latest active OTP for this email
        var otpToken = await _unitOfWork.OtpTokens.GetLatestActiveByEmailAsync(
            normalizedEmail,
            cancellationToken);

        if (otpToken == null || otpToken.Purpose != "registration")
        {
            _logger.LogWarning("No active registration OTP found for email {Email}", normalizedEmail);
            return new RegisterCompleteResultDto
            {
                Success = false,
                Error = "Verification code expired. Please request a new one.",
                RemainingAttempts = 0
            };
        }

        // Verify the code
        if (!otpToken.VerifyCode(request.Code))
        {
            await _unitOfWork.OtpTokens.UpdateAsync(otpToken, cancellationToken);

            var remainingAttempts = otpToken.MaxAttempts - otpToken.Attempts;
            _logger.LogWarning(
                "Invalid registration OTP attempt for email {Email}. Remaining attempts: {Remaining}",
                normalizedEmail,
                remainingAttempts);

            if (remainingAttempts <= 0)
            {
                // Clean up session on too many attempts
                await _cache.RemoveAsync($"registration:{request.SessionId}", cancellationToken);
                return new RegisterCompleteResultDto
                {
                    Success = false,
                    Error = "Too many invalid attempts. Please start registration again.",
                    RemainingAttempts = 0
                };
            }

            return new RegisterCompleteResultDto
            {
                Success = false,
                Error = "Invalid verification code.",
                RemainingAttempts = remainingAttempts
            };
        }

        // OTP is valid - mark as used
        await _unitOfWork.OtpTokens.UpdateAsync(otpToken, cancellationToken);

        // Double-check email is still available (race condition protection)
        if (await _unitOfWork.Users.EmailExistsAsync(normalizedEmail, cancellationToken))
        {
            await _cache.RemoveAsync($"registration:{request.SessionId}", cancellationToken);
            throw new ConflictException("An account with this email was just created. Please try logging in.");
        }

        Family family;
        User user;

        if (!string.IsNullOrEmpty(sessionData.InviteCode))
        {
            // TODO: Implement invite code validation when invite system is built
            // For now, look up family by invite code pattern: FAMILY_ID-RANDOM_CODE
            // This is a placeholder implementation that should be replaced with proper invite validation

            // Try to parse the invite code to get family ID (temporary implementation)
            var inviteParts = sessionData.InviteCode.Split('-');
            if (inviteParts.Length < 2)
            {
                await _cache.RemoveAsync($"registration:{request.SessionId}", cancellationToken);
                return new RegisterCompleteResultDto
                {
                    Success = false,
                    Error = "Invalid invite code format.",
                    RemainingAttempts = 0
                };
            }

            var familyId = inviteParts[0];
            family = await _unitOfWork.Families.GetByIdAsync(familyId, cancellationToken)
                ?? throw new NotFoundException($"Family not found for invite code.");

            // Create the user as an adult member of the existing family
            user = User.Create(
                family.Id,
                normalizedEmail,
                sessionData.DisplayName,
                UserRole.Adult);

            _logger.LogInformation(
                "User joining family {FamilyId} via invite code",
                family.Id);
        }
        else
        {
            // Create a new family (tenant)
            family = Family.Create(sessionData.FamilyName, sessionData.Timezone, normalizedEmail);
            await _unitOfWork.Families.AddAsync(family, cancellationToken);

            // Create the owner user
            user = User.CreateOwner(
                family.Id,
                normalizedEmail,
                sessionData.DisplayName);
        }

        user.EmailVerified = true; // Email is now verified via OTP
        user.RecordLogin();

        await _unitOfWork.Users.AddAsync(user, cancellationToken);

        // Create refresh token
        var (refreshToken, rawRefreshToken) = RefreshToken.Create(
            user.Id,
            user.FamilyId,
            TimeSpan.FromDays(7),
            request.IpAddress,
            request.UserAgent);

        await _unitOfWork.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Clean up registration session
        await _cache.RemoveAsync($"registration:{request.SessionId}", cancellationToken);

        // Generate authentication token
        var authResult = _tokenService.GenerateToken(user);
        authResult = authResult with { RefreshToken = rawRefreshToken };

        _logger.LogInformation(
            "User {UserId} registered successfully with verified email {Email}",
            user.Id,
            normalizedEmail);

        return new RegisterCompleteResultDto
        {
            Success = true,
            Family = new FamilyDto
            {
                Id = family.Id,
                Name = family.Name,
                Timezone = family.Timezone,
                Settings = new FamilySettingsDto
                {
                    DefaultView = family.Settings.DefaultView,
                    PrivacyModeEnabled = family.Settings.PrivacyModeEnabled,
                    PrivacyModeTimeoutMinutes = (int)family.Settings.PrivacyModeTimeout.TotalMinutes,
                    ShowWeather = family.Settings.ShowWeather,
                    WeatherLocation = family.Settings.WeatherLocation,
                    UseCelsius = family.Settings.UseCelsius,
                    WeekStartDay = family.Settings.WeekStartDay
                },
                CreatedAt = family.CreatedAt,
                MemberCount = 1,
                DeviceCount = 0
            },
            Auth = authResult,
            RemainingAttempts = 0
        };
    }
}
