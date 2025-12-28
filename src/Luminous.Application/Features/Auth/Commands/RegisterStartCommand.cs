using FluentValidation;
using Luminous.Application.Common.Exceptions;
using Luminous.Application.Common.Interfaces;
using Luminous.Application.DTOs;
using Luminous.Application.Features.Auth.Models;
using Luminous.Domain.Entities;
using Luminous.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using NanoidDotNet;
using System.Text.Json;

namespace Luminous.Application.Features.Auth.Commands;

/// <summary>
/// Command to start the registration process by sending an OTP to verify email ownership.
/// This is step 1 of the secure registration flow.
/// </summary>
public sealed record RegisterStartCommand : IRequest<RegisterStartResultDto>
{
    /// <summary>
    /// The name of the family to create.
    /// </summary>
    public string FamilyName { get; init; } = string.Empty;

    /// <summary>
    /// The timezone for the family (IANA timezone ID).
    /// </summary>
    public string Timezone { get; init; } = "UTC";

    /// <summary>
    /// The email address of the family owner.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// The display name of the family owner.
    /// </summary>
    public string DisplayName { get; init; } = string.Empty;

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
/// Validator for RegisterStartCommand.
/// </summary>
public sealed class RegisterStartCommandValidator : AbstractValidator<RegisterStartCommand>
{
    public RegisterStartCommandValidator()
    {
        RuleFor(x => x.FamilyName)
            .NotEmpty().WithMessage("Family name is required.")
            .MaximumLength(100).WithMessage("Family name must not exceed 100 characters.")
            .Matches(@"^[\w\s\-'\.]+$").WithMessage("Family name contains invalid characters.");

        RuleFor(x => x.Timezone)
            .NotEmpty().WithMessage("Timezone is required.")
            .MaximumLength(50).WithMessage("Timezone must not exceed 50 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters.");

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required.")
            .MaximumLength(50).WithMessage("Display name must not exceed 50 characters.")
            .Matches(@"^[\w\s\-'\.]+$").WithMessage("Display name contains invalid characters.");
    }
}

/// <summary>
/// Handler for RegisterStartCommand.
/// </summary>
public sealed class RegisterStartCommandHandler : IRequestHandler<RegisterStartCommand, RegisterStartResultDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IDistributedCache _cache;
    private readonly ILogger<RegisterStartCommandHandler> _logger;

    private static readonly TimeSpan SessionTimeout = TimeSpan.FromMinutes(15);
    private const int MaxRequestsPerHour = 5;
    private const int MinSecondsBetweenRequests = 60;

    public RegisterStartCommandHandler(
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IDistributedCache cache,
        ILogger<RegisterStartCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<RegisterStartResultDto> Handle(RegisterStartCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.ToLowerInvariant();

        // Check if email is already in use
        if (await _unitOfWork.Users.EmailExistsAsync(normalizedEmail, cancellationToken))
        {
            throw new ConflictException($"An account with this email already exists.");
        }

        // Check rate limiting
        var recentRequests = await _unitOfWork.OtpTokens.CountRecentRequestsAsync(
            normalizedEmail,
            TimeSpan.FromHours(1),
            cancellationToken);

        if (recentRequests >= MaxRequestsPerHour)
        {
            _logger.LogWarning("Registration rate limit exceeded for email {Email}", normalizedEmail);
            return new RegisterStartResultDto
            {
                Success = false,
                Message = "Too many registration attempts. Please try again later.",
                MaskedEmail = MaskEmail(normalizedEmail),
                RetryAfterSeconds = 3600
            };
        }

        // Check if there's a recent active OTP (prevent spam)
        var latestOtp = await _unitOfWork.OtpTokens.GetLatestActiveByEmailAsync(
            normalizedEmail,
            cancellationToken);

        if (latestOtp != null && latestOtp.Purpose == "registration")
        {
            var secondsSinceCreation = (DateTime.UtcNow - latestOtp.CreatedAt).TotalSeconds;
            if (secondsSinceCreation < MinSecondsBetweenRequests)
            {
                var retryAfter = (int)(MinSecondsBetweenRequests - secondsSinceCreation);
                return new RegisterStartResultDto
                {
                    Success = false,
                    Message = "Please wait before requesting a new verification code.",
                    MaskedEmail = MaskEmail(normalizedEmail),
                    ExpiresAt = latestOtp.ExpiresAt,
                    RetryAfterSeconds = retryAfter
                };
            }
        }

        // Invalidate any existing active OTPs for registration
        await _unitOfWork.OtpTokens.InvalidateAllForEmailAsync(normalizedEmail, cancellationToken);

        // Create OTP for registration
        var (otpToken, code) = OtpToken.Create(
            normalizedEmail,
            TimeSpan.FromMinutes(10),
            "registration",
            null, // No user ID yet
            null, // No family ID yet
            request.IpAddress,
            request.UserAgent);

        await _unitOfWork.OtpTokens.AddAsync(otpToken, cancellationToken);

        // Generate session ID and store registration data
        var sessionId = Nanoid.Generate(size: 21);
        var sessionData = new RegistrationSessionData
        {
            Email = normalizedEmail,
            DisplayName = request.DisplayName,
            FamilyName = request.FamilyName,
            Timezone = request.Timezone,
            CreatedAt = DateTime.UtcNow
        };

        await _cache.SetStringAsync(
            $"registration:{sessionId}",
            JsonSerializer.Serialize(sessionData),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = SessionTimeout },
            cancellationToken);

        // Send verification OTP via email
        try
        {
            await _emailService.SendOtpAsync(normalizedEmail, code, cancellationToken);
            _logger.LogInformation("Registration OTP sent to {Email}", normalizedEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send registration OTP to {Email}", normalizedEmail);
            // Clean up session on failure
            await _cache.RemoveAsync($"registration:{sessionId}", cancellationToken);
            throw new InvalidOperationException("Failed to send verification email. Please try again.");
        }

        return new RegisterStartResultDto
        {
            Success = true,
            SessionId = sessionId,
            Message = "Verification code sent to your email address.",
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
