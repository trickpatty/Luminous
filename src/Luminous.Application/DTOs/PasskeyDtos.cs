namespace Luminous.Application.DTOs;

/// <summary>
/// Result of starting a passkey registration.
/// </summary>
public sealed record PasskeyRegisterStartResultDto
{
    /// <summary>
    /// Gets the WebAuthn credential creation options as JSON.
    /// </summary>
    public string Options { get; init; } = string.Empty;

    /// <summary>
    /// Gets the session ID for completing the registration.
    /// </summary>
    public string SessionId { get; init; } = string.Empty;
}

/// <summary>
/// Result of completing a passkey registration.
/// </summary>
public sealed record PasskeyRegisterCompleteResultDto
{
    /// <summary>
    /// Gets whether the registration was successful.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets the credential ID of the registered passkey.
    /// </summary>
    public string? CredentialId { get; init; }

    /// <summary>
    /// Gets the error message if registration failed.
    /// </summary>
    public string? Error { get; init; }
}

/// <summary>
/// Result of starting a passkey authentication.
/// </summary>
public sealed record PasskeyAuthenticateStartResultDto
{
    /// <summary>
    /// Gets the WebAuthn assertion options as JSON.
    /// </summary>
    public string Options { get; init; } = string.Empty;

    /// <summary>
    /// Gets the session ID for completing the authentication.
    /// </summary>
    public string SessionId { get; init; } = string.Empty;
}

/// <summary>
/// Result of completing a passkey authentication.
/// </summary>
public sealed record PasskeyAuthenticateCompleteResultDto
{
    /// <summary>
    /// Gets whether the authentication was successful.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets the authentication result if successful.
    /// </summary>
    public AuthResultDto? Auth { get; init; }

    /// <summary>
    /// Gets the error message if authentication failed.
    /// </summary>
    public string? Error { get; init; }
}

/// <summary>
/// Information about a registered passkey.
/// </summary>
public sealed record PasskeyDto
{
    /// <summary>
    /// Gets the passkey ID.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Gets the credential ID (base64 encoded).
    /// </summary>
    public string CredentialId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the display name of the passkey.
    /// </summary>
    public string? DisplayName { get; init; }

    /// <summary>
    /// Gets when the passkey was registered.
    /// </summary>
    public DateTime RegisteredAt { get; init; }

    /// <summary>
    /// Gets when the passkey was last used.
    /// </summary>
    public DateTime? LastUsedAt { get; init; }

    /// <summary>
    /// Gets the AAGUID of the authenticator.
    /// </summary>
    public string AaGuid { get; init; } = string.Empty;

    /// <summary>
    /// Gets the transport hints.
    /// </summary>
    public List<string> Transports { get; init; } = [];

    /// <summary>
    /// Gets whether the passkey is active.
    /// </summary>
    public bool IsActive { get; init; }
}

/// <summary>
/// Result of listing passkeys.
/// </summary>
public sealed record PasskeyListResultDto
{
    /// <summary>
    /// Gets the list of passkeys.
    /// </summary>
    public List<PasskeyDto> Passkeys { get; init; } = [];

    /// <summary>
    /// Gets the total count of passkeys.
    /// </summary>
    public int TotalCount { get; init; }
}
