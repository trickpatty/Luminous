using Luminous.Domain.Entities;

namespace Luminous.Domain.Interfaces;

/// <summary>
/// Repository for managing OAuth sessions.
/// </summary>
public interface IOAuthSessionRepository
{
    /// <summary>
    /// Gets an OAuth session by its ID.
    /// </summary>
    Task<OAuthSession?> GetByIdAsync(string sessionId, string familyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an OAuth session by its state parameter.
    /// </summary>
    Task<OAuthSession?> GetByStateAsync(string state, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new OAuth session.
    /// </summary>
    Task<OAuthSession> AddAsync(OAuthSession session, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing OAuth session.
    /// </summary>
    Task UpdateAsync(OAuthSession session, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an OAuth session.
    /// </summary>
    Task DeleteAsync(string sessionId, string familyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all expired OAuth sessions.
    /// </summary>
    Task DeleteExpiredAsync(CancellationToken cancellationToken = default);
}
