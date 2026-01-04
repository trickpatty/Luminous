using Luminous.Domain.Enums;

namespace Luminous.Application.DTOs;

/// <summary>
/// Data transfer object for calendar connection.
/// </summary>
public record CalendarConnectionDto
{
    /// <summary>
    /// Calendar connection ID.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Family ID.
    /// </summary>
    public required string FamilyId { get; init; }

    /// <summary>
    /// Calendar name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Calendar provider type.
    /// </summary>
    public required CalendarProvider Provider { get; init; }

    /// <summary>
    /// Connection status.
    /// </summary>
    public required CalendarConnectionStatus Status { get; init; }

    /// <summary>
    /// External account email or identifier.
    /// </summary>
    public string? ExternalAccountId { get; init; }

    /// <summary>
    /// Assigned family member IDs.
    /// </summary>
    public List<string> AssignedMemberIds { get; init; } = [];

    /// <summary>
    /// Default color for events (hex code).
    /// </summary>
    public string? Color { get; init; }

    /// <summary>
    /// Whether the connection is enabled.
    /// </summary>
    public bool IsEnabled { get; init; }

    /// <summary>
    /// Whether the connection is read-only.
    /// </summary>
    public bool IsReadOnly { get; init; }

    /// <summary>
    /// Last successful sync time.
    /// </summary>
    public DateTime? LastSyncedAt { get; init; }

    /// <summary>
    /// Next scheduled sync time.
    /// </summary>
    public DateTime? NextSyncAt { get; init; }

    /// <summary>
    /// Last sync error message.
    /// </summary>
    public string? LastSyncError { get; init; }

    /// <summary>
    /// Number of consecutive failures.
    /// </summary>
    public int ConsecutiveFailures { get; init; }

    /// <summary>
    /// Sync settings.
    /// </summary>
    public CalendarSyncSettingsDto SyncSettings { get; init; } = new();

    /// <summary>
    /// Created timestamp.
    /// </summary>
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Summary DTO for calendar connection (lighter version).
/// </summary>
public record CalendarConnectionSummaryDto
{
    /// <summary>
    /// Calendar connection ID.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Calendar name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Calendar provider type.
    /// </summary>
    public required CalendarProvider Provider { get; init; }

    /// <summary>
    /// Connection status.
    /// </summary>
    public required CalendarConnectionStatus Status { get; init; }

    /// <summary>
    /// Whether the connection is enabled.
    /// </summary>
    public bool IsEnabled { get; init; }

    /// <summary>
    /// Default color (hex code).
    /// </summary>
    public string? Color { get; init; }

    /// <summary>
    /// Last successful sync time.
    /// </summary>
    public DateTime? LastSyncedAt { get; init; }
}

/// <summary>
/// DTO for calendar sync settings.
/// </summary>
public record CalendarSyncSettingsDto
{
    /// <summary>
    /// Sync interval in minutes.
    /// </summary>
    public int SyncIntervalMinutes { get; init; } = 15;

    /// <summary>
    /// Days to sync in the past.
    /// </summary>
    public int SyncPastDays { get; init; } = 7;

    /// <summary>
    /// Days to sync in the future.
    /// </summary>
    public int SyncFutureDays { get; init; } = 90;

    /// <summary>
    /// Whether to import all-day events.
    /// </summary>
    public bool ImportAllDayEvents { get; init; } = true;

    /// <summary>
    /// Whether to import declined events.
    /// </summary>
    public bool ImportDeclinedEvents { get; init; }

    /// <summary>
    /// Whether this is two-way sync.
    /// </summary>
    public bool TwoWaySync { get; init; }
}

/// <summary>
/// DTO for creating a calendar connection.
/// </summary>
public record CreateCalendarConnectionDto
{
    /// <summary>
    /// Calendar name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Calendar provider type.
    /// </summary>
    public required CalendarProvider Provider { get; init; }

    /// <summary>
    /// ICS URL (for ICS subscriptions only).
    /// </summary>
    public string? IcsUrl { get; init; }

    /// <summary>
    /// Assigned family member IDs.
    /// </summary>
    public List<string> AssignedMemberIds { get; init; } = [];

    /// <summary>
    /// Default color (hex code).
    /// </summary>
    public string? Color { get; init; }
}

/// <summary>
/// DTO for updating a calendar connection.
/// </summary>
public record UpdateCalendarConnectionDto
{
    /// <summary>
    /// Calendar name.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Assigned family member IDs.
    /// </summary>
    public List<string>? AssignedMemberIds { get; init; }

    /// <summary>
    /// Default color (hex code).
    /// </summary>
    public string? Color { get; init; }

    /// <summary>
    /// Whether the connection is enabled.
    /// </summary>
    public bool? IsEnabled { get; init; }

    /// <summary>
    /// Sync settings.
    /// </summary>
    public CalendarSyncSettingsDto? SyncSettings { get; init; }
}

/// <summary>
/// DTO for OAuth authorization response.
/// </summary>
public record CalendarOAuthStartDto
{
    /// <summary>
    /// Calendar connection ID.
    /// </summary>
    public required string ConnectionId { get; init; }

    /// <summary>
    /// OAuth authorization URL.
    /// </summary>
    public required string AuthorizationUrl { get; init; }
}

/// <summary>
/// DTO for available external calendars.
/// </summary>
public record AvailableCalendarDto
{
    /// <summary>
    /// External calendar ID.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Calendar name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Calendar description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Calendar color (hex code).
    /// </summary>
    public string? Color { get; init; }

    /// <summary>
    /// Whether the calendar is read-only.
    /// </summary>
    public bool IsReadOnly { get; init; }

    /// <summary>
    /// Whether this is the primary calendar.
    /// </summary>
    public bool IsPrimary { get; init; }
}
