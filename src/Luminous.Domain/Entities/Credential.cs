using Luminous.Domain.Common;

namespace Luminous.Domain.Entities;

/// <summary>
/// Represents a WebAuthn credential for passwordless authentication.
/// </summary>
public sealed class Credential : Entity
{
    /// <summary>
    /// Gets or sets the user ID this credential belongs to.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the credential ID (WebAuthn).
    /// </summary>
    public byte[] CredentialId { get; set; } = [];

    /// <summary>
    /// Gets or sets the public key (WebAuthn).
    /// </summary>
    public byte[] PublicKey { get; set; } = [];

    /// <summary>
    /// Gets or sets the user handle (WebAuthn).
    /// </summary>
    public byte[] UserHandle { get; set; } = [];

    /// <summary>
    /// Gets or sets the signature counter (WebAuthn).
    /// </summary>
    public uint SignatureCounter { get; set; }

    /// <summary>
    /// Gets or sets the credential type (public-key).
    /// </summary>
    public string CredentialType { get; set; } = "public-key";

    /// <summary>
    /// Gets or sets the registered timestamp.
    /// </summary>
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the AAGUID of the authenticator.
    /// </summary>
    public Guid AaGuid { get; set; }

    /// <summary>
    /// Gets or sets a friendly name for this credential.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets when this credential was last used.
    /// </summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>
    /// Gets or sets whether this credential is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets whether this is a discoverable credential (passkey).
    /// </summary>
    public bool IsDiscoverable { get; set; } = true;

    /// <summary>
    /// Gets or sets the transport hints (usb, nfc, ble, internal).
    /// </summary>
    public List<string> Transports { get; set; } = [];

    /// <summary>
    /// Creates a new credential.
    /// </summary>
    public static Credential Create(
        string userId,
        byte[] credentialId,
        byte[] publicKey,
        byte[] userHandle,
        uint signatureCounter,
        Guid aaGuid,
        string? displayName = null,
        bool isDiscoverable = true)
    {
        return new Credential
        {
            UserId = userId,
            CredentialId = credentialId,
            PublicKey = publicKey,
            UserHandle = userHandle,
            SignatureCounter = signatureCounter,
            AaGuid = aaGuid,
            DisplayName = displayName,
            IsDiscoverable = isDiscoverable,
            CreatedBy = userId
        };
    }

    /// <summary>
    /// Updates the signature counter after successful authentication.
    /// </summary>
    public void UpdateCounter(uint newCounter)
    {
        SignatureCounter = newCounter;
        LastUsedAt = DateTime.UtcNow;
        ModifiedAt = DateTime.UtcNow;
    }
}
