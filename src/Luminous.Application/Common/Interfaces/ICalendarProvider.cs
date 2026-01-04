using Luminous.Domain.Enums;
using Luminous.Domain.ValueObjects;

namespace Luminous.Application.Common.Interfaces;

/// <summary>
/// Interface for calendar provider implementations (Google, Outlook, ICS, etc.).
/// </summary>
public interface ICalendarProvider
{
    /// <summary>
    /// Gets the calendar provider type.
    /// </summary>
    CalendarProvider ProviderType { get; }

    /// <summary>
    /// Gets whether this provider supports OAuth authentication.
    /// </summary>
    bool RequiresOAuth { get; }

    /// <summary>
    /// Gets whether this provider supports two-way sync.
    /// </summary>
    bool SupportsTwoWaySync { get; }

    /// <summary>
    /// Gets the OAuth authorization URL for the user to authenticate.
    /// </summary>
    /// <param name="state">State parameter for OAuth flow (should include connection ID).</param>
    /// <param name="redirectUri">The redirect URI after authentication.</param>
    /// <returns>The authorization URL.</returns>
    Task<string> GetAuthorizationUrlAsync(string state, string redirectUri);

    /// <summary>
    /// Exchanges an authorization code for tokens.
    /// </summary>
    /// <param name="code">The authorization code.</param>
    /// <param name="redirectUri">The redirect URI used in the auth request.</param>
    /// <returns>The OAuth tokens.</returns>
    Task<OAuthTokens> ExchangeCodeAsync(string code, string redirectUri);

    /// <summary>
    /// Refreshes the access token using the refresh token.
    /// </summary>
    /// <param name="tokens">The current tokens with refresh token.</param>
    /// <returns>The new OAuth tokens.</returns>
    Task<OAuthTokens> RefreshTokensAsync(OAuthTokens tokens);

    /// <summary>
    /// Revokes access tokens.
    /// </summary>
    /// <param name="tokens">The tokens to revoke.</param>
    Task RevokeTokensAsync(OAuthTokens tokens);

    /// <summary>
    /// Gets the account email/identifier for the authenticated user.
    /// </summary>
    /// <param name="tokens">The OAuth tokens.</param>
    /// <returns>The account email address.</returns>
    Task<string> GetAccountEmailAsync(OAuthTokens tokens);

    /// <summary>
    /// Gets the list of calendars available for the authenticated user.
    /// </summary>
    /// <param name="tokens">The OAuth tokens.</param>
    /// <returns>List of available calendars.</returns>
    Task<IReadOnlyList<ExternalCalendarInfo>> GetCalendarsAsync(OAuthTokens tokens);

    /// <summary>
    /// Fetches events from the calendar.
    /// </summary>
    /// <param name="tokens">The OAuth tokens (or null for ICS subscriptions).</param>
    /// <param name="calendarId">The external calendar ID.</param>
    /// <param name="startDate">Start of the date range.</param>
    /// <param name="endDate">End of the date range.</param>
    /// <param name="syncToken">Optional sync token for incremental sync.</param>
    /// <returns>The sync result with events.</returns>
    Task<CalendarSyncResult> FetchEventsAsync(
        OAuthTokens? tokens,
        string calendarId,
        DateTime startDate,
        DateTime endDate,
        string? syncToken = null);

    /// <summary>
    /// Fetches events from an ICS URL (for ICS provider only).
    /// </summary>
    /// <param name="icsUrl">The ICS URL to fetch.</param>
    /// <param name="etag">Optional ETag for conditional fetch.</param>
    /// <returns>The sync result with events.</returns>
    Task<CalendarSyncResult> FetchIcsEventsAsync(string icsUrl, string? etag = null);

    /// <summary>
    /// Creates an event in the external calendar (for two-way sync).
    /// </summary>
    Task<string> CreateEventAsync(OAuthTokens tokens, string calendarId, ExternalCalendarEvent calendarEvent);

    /// <summary>
    /// Updates an event in the external calendar (for two-way sync).
    /// </summary>
    Task UpdateEventAsync(OAuthTokens tokens, string calendarId, string eventId, ExternalCalendarEvent calendarEvent);

    /// <summary>
    /// Deletes an event from the external calendar (for two-way sync).
    /// </summary>
    Task DeleteEventAsync(OAuthTokens tokens, string calendarId, string eventId);
}

/// <summary>
/// Information about an external calendar.
/// </summary>
public record ExternalCalendarInfo(
    string Id,
    string Name,
    string? Description,
    string? Color,
    bool IsReadOnly,
    bool IsPrimary,
    string? TimeZone);

/// <summary>
/// Result of a calendar sync operation.
/// </summary>
public record CalendarSyncResult(
    IReadOnlyList<ExternalCalendarEvent> Events,
    IReadOnlyList<string> DeletedEventIds,
    string? SyncToken,
    string? ETag,
    bool FullSyncRequired);

/// <summary>
/// An event from an external calendar.
/// </summary>
public record ExternalCalendarEvent
{
    /// <summary>
    /// External event ID from the provider.
    /// </summary>
    public required string ExternalId { get; init; }

    /// <summary>
    /// Event title.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Event description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Event start time.
    /// </summary>
    public required DateTime StartTime { get; init; }

    /// <summary>
    /// Event end time.
    /// </summary>
    public required DateTime EndTime { get; init; }

    /// <summary>
    /// Whether this is an all-day event.
    /// </summary>
    public bool IsAllDay { get; init; }

    /// <summary>
    /// Event location text.
    /// </summary>
    public string? Location { get; init; }

    /// <summary>
    /// Event color (hex code).
    /// </summary>
    public string? Color { get; init; }

    /// <summary>
    /// Recurrence rule (RRULE format).
    /// </summary>
    public string? RecurrenceRule { get; init; }

    /// <summary>
    /// ID of the recurring event series (for instances).
    /// </summary>
    public string? RecurringEventId { get; init; }

    /// <summary>
    /// Original start time for a recurring event instance.
    /// </summary>
    public DateTime? OriginalStartTime { get; init; }

    /// <summary>
    /// Whether the event was cancelled.
    /// </summary>
    public bool IsCancelled { get; init; }

    /// <summary>
    /// Reminder minutes before the event.
    /// </summary>
    public List<int> Reminders { get; init; } = [];

    /// <summary>
    /// Last modification time.
    /// </summary>
    public DateTime? UpdatedAt { get; init; }

    /// <summary>
    /// Event organizer email.
    /// </summary>
    public string? OrganizerEmail { get; init; }
}
