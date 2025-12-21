using Luminous.Domain.Entities;

namespace Luminous.Domain.Interfaces;

/// <summary>
/// Repository interface for WebAuthn Credential entities.
/// </summary>
public interface ICredentialRepository : IRepository<Credential>
{
    /// <summary>
    /// Gets all credentials for a user.
    /// </summary>
    Task<IReadOnlyList<Credential>> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a credential by its credential ID.
    /// </summary>
    Task<Credential?> GetByCredentialIdAsync(
        byte[] credentialId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a credential by user handle.
    /// </summary>
    Task<Credential?> GetByUserHandleAsync(
        byte[] userHandle,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a credential ID already exists.
    /// </summary>
    Task<bool> CredentialIdExistsAsync(
        byte[] credentialId,
        CancellationToken cancellationToken = default);
}
