namespace Luminous.Application.DTOs;

/// <summary>
/// Result of an authentication operation.
/// </summary>
public sealed record AuthResultDto
{
    /// <summary>
    /// Gets the JWT access token.
    /// </summary>
    public string AccessToken { get; init; } = string.Empty;

    /// <summary>
    /// Gets the refresh token for obtaining new access tokens.
    /// </summary>
    public string? RefreshToken { get; init; }

    /// <summary>
    /// Gets the token type (typically "Bearer").
    /// </summary>
    public string TokenType { get; init; } = "Bearer";

    /// <summary>
    /// Gets the expiration time in seconds.
    /// </summary>
    public int ExpiresIn { get; init; }

    /// <summary>
    /// Gets the authenticated user information.
    /// </summary>
    public AuthUserDto User { get; init; } = new();
}

/// <summary>
/// Authenticated user information returned after login.
/// </summary>
public sealed record AuthUserDto
{
    /// <summary>
    /// Gets the user ID.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Gets the family ID the user belongs to.
    /// </summary>
    public string FamilyId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the user's email address.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Gets the user's display name.
    /// </summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the user's role.
    /// </summary>
    public string Role { get; init; } = string.Empty;

    /// <summary>
    /// Gets whether this is the user's first login.
    /// </summary>
    public bool IsFirstLogin { get; init; }
}

/// <summary>
/// Result of family creation including auth token for immediate login.
/// </summary>
public sealed record FamilyCreationResultDto
{
    /// <summary>
    /// Gets the created family.
    /// </summary>
    public FamilyDto Family { get; init; } = new();

    /// <summary>
    /// Gets the authentication result for the owner.
    /// </summary>
    public AuthResultDto Auth { get; init; } = new();
}

/// <summary>
/// Result of starting the registration process.
/// </summary>
public sealed record RegisterStartResultDto
{
    /// <summary>
    /// Whether the operation succeeded.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// The session ID for completing registration.
    /// </summary>
    public string? SessionId { get; init; }

    /// <summary>
    /// Message describing the result.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Masked email address for display.
    /// </summary>
    public string MaskedEmail { get; init; } = string.Empty;

    /// <summary>
    /// When the OTP expires.
    /// </summary>
    public DateTime? ExpiresAt { get; init; }

    /// <summary>
    /// Seconds until a new OTP can be requested.
    /// </summary>
    public int? RetryAfterSeconds { get; init; }
}

/// <summary>
/// Result of completing the registration process.
/// </summary>
public sealed record RegisterCompleteResultDto
{
    /// <summary>
    /// Whether the operation succeeded.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Error message if registration failed.
    /// </summary>
    public string? Error { get; init; }

    /// <summary>
    /// The created family details.
    /// </summary>
    public FamilyDto? Family { get; init; }

    /// <summary>
    /// The authentication result for the owner.
    /// </summary>
    public AuthResultDto? Auth { get; init; }

    /// <summary>
    /// Remaining OTP verification attempts.
    /// </summary>
    public int RemainingAttempts { get; init; }
}
