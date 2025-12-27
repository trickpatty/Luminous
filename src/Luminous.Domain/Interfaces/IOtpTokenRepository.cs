using Luminous.Domain.Entities;

namespace Luminous.Domain.Interfaces;

/// <summary>
/// Repository interface for OTP Token entities.
/// </summary>
public interface IOtpTokenRepository : IRepository<OtpToken>
{
    /// <summary>
    /// Gets all active OTP tokens for an email.
    /// </summary>
    Task<IReadOnlyList<OtpToken>> GetActiveByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the most recent active OTP token for an email.
    /// </summary>
    Task<OtpToken?> GetLatestActiveByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates all active OTP tokens for an email.
    /// </summary>
    Task InvalidateAllForEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes expired OTP tokens older than the specified date.
    /// </summary>
    Task DeleteExpiredAsync(
        DateTime olderThan,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts recent OTP requests for rate limiting.
    /// </summary>
    Task<int> CountRecentRequestsAsync(
        string email,
        TimeSpan window,
        CancellationToken cancellationToken = default);
}
