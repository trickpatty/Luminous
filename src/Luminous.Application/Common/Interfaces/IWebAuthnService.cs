using Fido2NetLib;
using Fido2NetLib.Objects;

namespace Luminous.Application.Common.Interfaces;

/// <summary>
/// Service for WebAuthn/FIDO2 passkey operations.
/// </summary>
public interface IWebAuthnService
{
    /// <summary>
    /// Creates credential creation options for passkey registration.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="email">The user's email.</param>
    /// <param name="displayName">The user's display name.</param>
    /// <param name="existingCredentialIds">IDs of existing credentials to exclude.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The credential creation options and a session ID.</returns>
    Task<(CredentialCreateOptions Options, string SessionId)> CreateRegistrationOptionsAsync(
        string userId,
        string email,
        string displayName,
        IEnumerable<byte[]>? existingCredentialIds = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies and stores a new passkey credential.
    /// </summary>
    /// <param name="sessionId">The session ID from registration start.</param>
    /// <param name="attestationResponse">The attestation response from the authenticator.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The registered credential result.</returns>
    Task<RegisteredCredentialResult> CompleteRegistrationAsync(
        string sessionId,
        AuthenticatorAttestationRawResponse attestationResponse,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates assertion options for passkey authentication.
    /// </summary>
    /// <param name="email">The user's email (optional for discoverable credentials).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The assertion options and a session ID.</returns>
    Task<(AssertionOptions Options, string SessionId)> CreateAuthenticationOptionsAsync(
        string? email = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies a passkey authentication assertion.
    /// </summary>
    /// <param name="sessionId">The session ID from authentication start.</param>
    /// <param name="assertionResponse">The assertion response from the authenticator.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The authentication result.</returns>
    Task<AuthenticationResult> CompleteAuthenticationAsync(
        string sessionId,
        AuthenticatorAssertionRawResponse assertionResponse,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of a successful passkey registration.
/// </summary>
public sealed record RegisteredCredentialResult
{
    /// <summary>
    /// Gets whether the registration was successful.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets the credential ID.
    /// </summary>
    public byte[]? CredentialId { get; init; }

    /// <summary>
    /// Gets the public key.
    /// </summary>
    public byte[]? PublicKey { get; init; }

    /// <summary>
    /// Gets the user handle.
    /// </summary>
    public byte[]? UserHandle { get; init; }

    /// <summary>
    /// Gets the signature counter.
    /// </summary>
    public uint SignatureCounter { get; init; }

    /// <summary>
    /// Gets the AAGUID.
    /// </summary>
    public Guid AaGuid { get; init; }

    /// <summary>
    /// Gets the credential type.
    /// </summary>
    public string CredentialType { get; init; } = "public-key";

    /// <summary>
    /// Gets the user ID.
    /// </summary>
    public string? UserId { get; init; }

    /// <summary>
    /// Gets the error message if registration failed.
    /// </summary>
    public string? Error { get; init; }
}

/// <summary>
/// Result of a successful passkey authentication.
/// </summary>
public sealed record AuthenticationResult
{
    /// <summary>
    /// Gets whether the authentication was successful.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets the user ID of the authenticated user.
    /// </summary>
    public string? UserId { get; init; }

    /// <summary>
    /// Gets the credential ID used for authentication.
    /// </summary>
    public byte[]? CredentialId { get; init; }

    /// <summary>
    /// Gets the new signature counter.
    /// </summary>
    public uint SignatureCounter { get; init; }

    /// <summary>
    /// Gets the error message if authentication failed.
    /// </summary>
    public string? Error { get; init; }
}
