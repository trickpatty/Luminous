namespace Luminous.Application.DTOs;

/// <summary>
/// Result of an OTP request operation.
/// </summary>
public sealed record OtpRequestResultDto
{
    /// <summary>
    /// Gets whether the OTP was sent successfully.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets the message for the user.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Gets the email the OTP was sent to (masked).
    /// </summary>
    public string MaskedEmail { get; init; } = string.Empty;

    /// <summary>
    /// Gets when the OTP expires.
    /// </summary>
    public DateTime ExpiresAt { get; init; }

    /// <summary>
    /// Gets the number of seconds until a new OTP can be requested.
    /// </summary>
    public int RetryAfterSeconds { get; init; }
}

/// <summary>
/// Result of an OTP verification operation.
/// </summary>
public sealed record OtpVerifyResultDto
{
    /// <summary>
    /// Gets whether the OTP was verified successfully.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets the authentication result if successful.
    /// </summary>
    public AuthResultDto? Auth { get; init; }

    /// <summary>
    /// Gets the error message if verification failed.
    /// </summary>
    public string? Error { get; init; }

    /// <summary>
    /// Gets the number of remaining attempts.
    /// </summary>
    public int RemainingAttempts { get; init; }
}
