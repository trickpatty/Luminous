using Luminous.Domain.Entities;
using Luminous.Domain.Enums;

namespace Luminous.Application.Common.Interfaces;

/// <summary>
/// High-level service for managing calendar synchronization.
/// </summary>
public interface ICalendarSyncService
{
    /// <summary>
    /// Syncs events from a calendar connection.
    /// </summary>
    /// <param name="connection">The calendar connection to sync.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The sync result summary.</returns>
    Task<CalendarSyncSummary> SyncAsync(
        CalendarConnection connection,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Syncs all due calendar connections.
    /// </summary>
    /// <param name="limit">Maximum number of connections to sync.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of sync results.</returns>
    Task<IReadOnlyList<CalendarSyncSummary>> SyncDueConnectionsAsync(
        int limit = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the OAuth authorization URL for a provider.
    /// </summary>
    /// <param name="provider">The calendar provider.</param>
    /// <param name="connectionId">The calendar connection ID.</param>
    /// <param name="familyId">The family ID.</param>
    /// <param name="redirectUri">The redirect URI.</param>
    /// <returns>The authorization URL.</returns>
    Task<string> GetAuthorizationUrlAsync(
        CalendarProvider provider,
        string connectionId,
        string familyId,
        string redirectUri);

    /// <summary>
    /// Completes the OAuth flow by exchanging the authorization code.
    /// </summary>
    /// <param name="connectionId">The calendar connection ID.</param>
    /// <param name="code">The authorization code.</param>
    /// <param name="redirectUri">The redirect URI.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task CompleteOAuthAsync(
        string connectionId,
        string code,
        string redirectUri,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes tokens for a connection if needed.
    /// </summary>
    /// <param name="connection">The calendar connection.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if tokens were refreshed.</returns>
    Task<bool> RefreshTokensIfNeededAsync(
        CalendarConnection connection,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets available calendars from a provider.
    /// </summary>
    /// <param name="connection">The calendar connection.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of available calendars.</returns>
    Task<IReadOnlyList<ExternalCalendarInfo>> GetAvailableCalendarsAsync(
        CalendarConnection connection,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Summary of a calendar sync operation.
/// </summary>
public record CalendarSyncSummary
{
    /// <summary>
    /// The calendar connection ID.
    /// </summary>
    public required string ConnectionId { get; init; }

    /// <summary>
    /// The family ID.
    /// </summary>
    public required string FamilyId { get; init; }

    /// <summary>
    /// Whether the sync was successful.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Number of events added.
    /// </summary>
    public int EventsAdded { get; init; }

    /// <summary>
    /// Number of events updated.
    /// </summary>
    public int EventsUpdated { get; init; }

    /// <summary>
    /// Number of events deleted.
    /// </summary>
    public int EventsDeleted { get; init; }

    /// <summary>
    /// Error message if sync failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Whether the error was an authentication error.
    /// </summary>
    public bool IsAuthError { get; init; }

    /// <summary>
    /// Duration of the sync operation.
    /// </summary>
    public TimeSpan Duration { get; init; }
}
