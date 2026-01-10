namespace Luminous.Application.DTOs;

/// <summary>
/// Types of sync messages sent via SignalR.
/// </summary>
public enum SyncMessageType
{
    // Event-related messages
    EventCreated,
    EventUpdated,
    EventDeleted,
    EventsRefreshed,

    // Chore-related messages
    ChoreCreated,
    ChoreUpdated,
    ChoreDeleted,
    ChoreCompleted,

    // User-related messages
    UserUpdated,
    UserJoined,
    UserLeft,

    // Family-related messages
    FamilyUpdated,
    FamilySettingsUpdated,

    // Device-related messages
    DeviceLinked,
    DeviceUnlinked,
    DeviceUpdated,

    // Calendar-related messages
    CalendarConnectionAdded,
    CalendarConnectionRemoved,
    CalendarSyncCompleted,

    // General sync messages
    FullSyncRequired,
    HeartbeatResponse
}

/// <summary>
/// Base DTO for all sync messages sent via SignalR.
/// </summary>
public sealed record SyncMessageDto
{
    /// <summary>
    /// The type of sync message.
    /// </summary>
    public SyncMessageType Type { get; init; }

    /// <summary>
    /// The family ID this message belongs to.
    /// </summary>
    public string FamilyId { get; init; } = string.Empty;

    /// <summary>
    /// The ID of the entity that was affected (optional).
    /// </summary>
    public string? EntityId { get; init; }

    /// <summary>
    /// The ID of the user who triggered the change (optional).
    /// </summary>
    public string? TriggeredBy { get; init; }

    /// <summary>
    /// Timestamp when the message was created (UTC).
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Optional payload containing the full entity data.
    /// </summary>
    public object? Payload { get; init; }
}

/// <summary>
/// Sync message specifically for event changes.
/// </summary>
public sealed record EventSyncMessageDto
{
    /// <summary>
    /// The type of change (Created, Updated, Deleted).
    /// </summary>
    public SyncMessageType Type { get; init; }

    /// <summary>
    /// The ID of the event that changed.
    /// </summary>
    public string EventId { get; init; } = string.Empty;

    /// <summary>
    /// The full event data (for Created/Updated), null for Deleted.
    /// </summary>
    public EventSummaryDto? Event { get; init; }

    /// <summary>
    /// Timestamp when the change occurred (UTC).
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Sync message specifically for chore changes.
/// </summary>
public sealed record ChoreSyncMessageDto
{
    /// <summary>
    /// The type of change (Created, Updated, Deleted, Completed).
    /// </summary>
    public SyncMessageType Type { get; init; }

    /// <summary>
    /// The ID of the chore that changed.
    /// </summary>
    public string ChoreId { get; init; } = string.Empty;

    /// <summary>
    /// The full chore data (for Created/Updated), null for Deleted.
    /// </summary>
    public ChoreDto? Chore { get; init; }

    /// <summary>
    /// For completed chores, the user who completed it.
    /// </summary>
    public string? CompletedBy { get; init; }

    /// <summary>
    /// Timestamp when the change occurred (UTC).
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Sync message for calendar sync completion.
/// </summary>
public sealed record CalendarSyncCompletedMessageDto
{
    /// <summary>
    /// The ID of the calendar connection that was synced.
    /// </summary>
    public string CalendarConnectionId { get; init; } = string.Empty;

    /// <summary>
    /// The provider name (Google, Microsoft, ICS).
    /// </summary>
    public string Provider { get; init; } = string.Empty;

    /// <summary>
    /// Number of events added during sync.
    /// </summary>
    public int EventsAdded { get; init; }

    /// <summary>
    /// Number of events updated during sync.
    /// </summary>
    public int EventsUpdated { get; init; }

    /// <summary>
    /// Number of events removed during sync.
    /// </summary>
    public int EventsRemoved { get; init; }

    /// <summary>
    /// Timestamp when the sync completed (UTC).
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Sync message for user-related changes.
/// </summary>
public sealed record UserSyncMessageDto
{
    /// <summary>
    /// The type of change (Updated, Joined, Left).
    /// </summary>
    public SyncMessageType Type { get; init; }

    /// <summary>
    /// The ID of the user that changed.
    /// </summary>
    public string UserId { get; init; } = string.Empty;

    /// <summary>
    /// The user's display name.
    /// </summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>
    /// Timestamp when the change occurred (UTC).
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
