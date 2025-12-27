using Luminous.Domain.Entities;

namespace Luminous.Domain.Interfaces;

/// <summary>
/// Repository interface for Refresh Token entities.
/// </summary>
public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    /// <summary>
    /// Gets all active refresh tokens for a user.
    /// </summary>
    Task<IReadOnlyList<RefreshToken>> GetActiveByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a refresh token by its hash.
    /// </summary>
    Task<RefreshToken?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes all active tokens for a user.
    /// </summary>
    Task RevokeAllForUserAsync(
        string userId,
        string reason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes all active tokens for a user except the specified one.
    /// </summary>
    Task RevokeAllExceptAsync(
        string userId,
        string exceptTokenId,
        string reason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes all active tokens for a device.
    /// </summary>
    Task RevokeAllForDeviceAsync(
        string deviceId,
        string reason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes expired and revoked tokens older than the specified date.
    /// </summary>
    Task DeleteExpiredAsync(
        DateTime olderThan,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts active tokens for a user (for session limiting).
    /// </summary>
    Task<int> CountActiveForUserAsync(
        string userId,
        CancellationToken cancellationToken = default);
}
