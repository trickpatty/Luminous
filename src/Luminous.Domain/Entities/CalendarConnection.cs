using Luminous.Domain.Common;
using Luminous.Domain.Enums;
using Luminous.Domain.ValueObjects;

namespace Luminous.Domain.Entities;

/// <summary>
/// Represents a connection to an external calendar source.
/// </summary>
public sealed class CalendarConnection : Entity
{
    /// <summary>
    /// Gets or sets the family ID this connection belongs to (partition key).
    /// </summary>
    public string FamilyId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user-friendly name for this calendar.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the calendar provider type.
    /// </summary>
    public CalendarProvider Provider { get; set; }

    /// <summary>
    /// Gets or sets the connection status.
    /// </summary>
    public CalendarConnectionStatus Status { get; set; } = CalendarConnectionStatus.PendingAuth;

    /// <summary>
    /// Gets or sets the external calendar ID from the provider.
    /// </summary>
    public string? ExternalCalendarId { get; set; }

    /// <summary>
    /// Gets or sets the external account email or identifier.
    /// </summary>
    public string? ExternalAccountId { get; set; }

    /// <summary>
    /// Gets or sets the OAuth tokens for authenticated providers.
    /// </summary>
    public OAuthTokens? Tokens { get; set; }

    /// <summary>
    /// Gets or sets the ICS URL for subscription calendars.
    /// </summary>
    public string? IcsUrl { get; set; }

    /// <summary>
    /// Gets or sets the assigned family member IDs.
    /// When empty, events are assigned to no specific member.
    /// </summary>
    public List<string> AssignedMemberIds { get; set; } = [];

    /// <summary>
    /// Gets or sets the default color for events from this calendar (hex code).
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Gets or sets the sync settings.
    /// </summary>
    public CalendarSyncSettings SyncSettings { get; set; } = new();

    /// <summary>
    /// Gets or sets whether the connection is enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the last successful sync time.
    /// </summary>
    public DateTime? LastSyncedAt { get; set; }

    /// <summary>
    /// Gets or sets the next scheduled sync time.
    /// </summary>
    public DateTime? NextSyncAt { get; set; }

    /// <summary>
    /// Gets or sets the last sync error message.
    /// </summary>
    public string? LastSyncError { get; set; }

    /// <summary>
    /// Gets or sets the number of consecutive sync failures.
    /// </summary>
    public int ConsecutiveFailures { get; set; }

    /// <summary>
    /// Gets or sets the sync token/delta token for incremental sync.
    /// </summary>
    public string? SyncToken { get; set; }

    /// <summary>
    /// Gets or sets the ETag/version for ICS subscriptions.
    /// </summary>
    public string? ETag { get; set; }

    /// <summary>
    /// Gets whether this connection requires OAuth authentication.
    /// </summary>
    public bool RequiresOAuth => Provider is CalendarProvider.Google or CalendarProvider.Outlook;

    /// <summary>
    /// Gets whether this connection is read-only.
    /// </summary>
    public bool IsReadOnly => Provider == CalendarProvider.IcsUrl || !SyncSettings.TwoWaySync;

    /// <summary>
    /// Creates a new calendar connection for OAuth-based providers.
    /// </summary>
    public static CalendarConnection CreateOAuth(
        string familyId,
        CalendarProvider provider,
        string name,
        string createdBy)
    {
        if (string.IsNullOrWhiteSpace(familyId))
            throw new ArgumentException("Family ID is required.", nameof(familyId));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));
        if (provider is not (CalendarProvider.Google or CalendarProvider.Outlook))
            throw new ArgumentException($"Provider {provider} does not support OAuth.", nameof(provider));

        return new CalendarConnection
        {
            FamilyId = familyId,
            Name = name.Trim(),
            Provider = provider,
            Status = CalendarConnectionStatus.PendingAuth,
            CreatedBy = createdBy
        };
    }

    /// <summary>
    /// Creates a new calendar connection for ICS URL subscription.
    /// </summary>
    public static CalendarConnection CreateIcsSubscription(
        string familyId,
        string name,
        string icsUrl,
        string createdBy)
    {
        if (string.IsNullOrWhiteSpace(familyId))
            throw new ArgumentException("Family ID is required.", nameof(familyId));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(icsUrl))
            throw new ArgumentException("ICS URL is required.", nameof(icsUrl));
        if (!Uri.TryCreate(icsUrl, UriKind.Absolute, out var uri) ||
            uri.Scheme is not ("http" or "https" or "webcal"))
            throw new ArgumentException("Invalid ICS URL format.", nameof(icsUrl));

        // Normalize webcal:// to https://
        var normalizedUrl = icsUrl.StartsWith("webcal://", StringComparison.OrdinalIgnoreCase)
            ? "https://" + icsUrl[9..]
            : icsUrl;

        return new CalendarConnection
        {
            FamilyId = familyId,
            Name = name.Trim(),
            Provider = CalendarProvider.IcsUrl,
            IcsUrl = normalizedUrl,
            Status = CalendarConnectionStatus.Active,
            SyncSettings = CalendarSyncSettings.ForIcsSubscription(),
            CreatedBy = createdBy
        };
    }

    /// <summary>
    /// Completes the OAuth flow by storing tokens and activating the connection.
    /// </summary>
    public void CompleteOAuth(
        OAuthTokens tokens,
        string externalCalendarId,
        string? externalAccountId = null,
        string? calendarName = null)
    {
        if (tokens is null)
            throw new ArgumentNullException(nameof(tokens));
        if (string.IsNullOrWhiteSpace(externalCalendarId))
            throw new ArgumentException("External calendar ID is required.", nameof(externalCalendarId));

        Tokens = tokens;
        ExternalCalendarId = externalCalendarId;
        ExternalAccountId = externalAccountId;
        Status = CalendarConnectionStatus.Active;

        if (!string.IsNullOrWhiteSpace(calendarName))
            Name = calendarName;

        ScheduleNextSync();
    }

    /// <summary>
    /// Updates the OAuth tokens.
    /// </summary>
    public void UpdateTokens(OAuthTokens tokens)
    {
        Tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));

        if (Status == CalendarConnectionStatus.AuthError)
            Status = CalendarConnectionStatus.Active;
    }

    /// <summary>
    /// Records a successful sync.
    /// </summary>
    public void RecordSuccessfulSync(string? newSyncToken = null, string? etag = null)
    {
        LastSyncedAt = DateTime.UtcNow;
        LastSyncError = null;
        ConsecutiveFailures = 0;
        Status = CalendarConnectionStatus.Active;

        if (newSyncToken is not null)
            SyncToken = newSyncToken;
        if (etag is not null)
            ETag = etag;

        ScheduleNextSync();
    }

    /// <summary>
    /// Records a sync failure.
    /// </summary>
    public void RecordSyncFailure(string error, bool isAuthError = false)
    {
        LastSyncError = error;
        ConsecutiveFailures++;
        Status = isAuthError
            ? CalendarConnectionStatus.AuthError
            : CalendarConnectionStatus.SyncError;

        // Exponential backoff: wait longer after consecutive failures
        var backoffMinutes = Math.Min(
            SyncSettings.SyncIntervalMinutes * Math.Pow(2, ConsecutiveFailures - 1),
            1440); // Max 24 hours

        NextSyncAt = DateTime.UtcNow.AddMinutes(backoffMinutes);
    }

    /// <summary>
    /// Assigns this calendar to family members.
    /// </summary>
    public void AssignToMembers(IEnumerable<string> memberIds)
    {
        AssignedMemberIds = memberIds?.ToList() ?? [];
    }

    /// <summary>
    /// Enables the connection.
    /// </summary>
    public void Enable()
    {
        IsEnabled = true;
        if (Status == CalendarConnectionStatus.Paused)
            Status = CalendarConnectionStatus.Active;

        ScheduleNextSync();
    }

    /// <summary>
    /// Pauses the connection.
    /// </summary>
    public void Pause()
    {
        IsEnabled = false;
        Status = CalendarConnectionStatus.Paused;
        NextSyncAt = null;
    }

    /// <summary>
    /// Disconnects the calendar (revokes access).
    /// </summary>
    public void Disconnect()
    {
        IsEnabled = false;
        Status = CalendarConnectionStatus.Disconnected;
        Tokens = null;
        SyncToken = null;
        NextSyncAt = null;
    }

    private void ScheduleNextSync()
    {
        if (IsEnabled && Status == CalendarConnectionStatus.Active)
        {
            NextSyncAt = DateTime.UtcNow.AddMinutes(SyncSettings.SyncIntervalMinutes);
        }
    }
}
