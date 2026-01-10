using Luminous.Application.DTOs;

namespace Luminous.Application.Common.Interfaces;

/// <summary>
/// Service for sending real-time sync notifications to connected clients.
/// </summary>
public interface ISyncNotificationService
{
    /// <summary>
    /// Sends a generic sync message to all connected clients in a family.
    /// </summary>
    /// <param name="familyId">The family ID to send the message to.</param>
    /// <param name="message">The sync message to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendToFamilyAsync(string familyId, SyncMessageDto message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies the family that an event was created.
    /// </summary>
    Task NotifyEventCreatedAsync(string familyId, EventSummaryDto eventData, string? triggeredBy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies the family that an event was updated.
    /// </summary>
    Task NotifyEventUpdatedAsync(string familyId, EventSummaryDto eventData, string? triggeredBy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies the family that an event was deleted.
    /// </summary>
    Task NotifyEventDeletedAsync(string familyId, string eventId, string? triggeredBy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies the family that events have been refreshed (e.g., after a calendar sync).
    /// Clients should re-fetch their event data.
    /// </summary>
    Task NotifyEventsRefreshedAsync(string familyId, string? triggeredBy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies the family that a chore was created.
    /// </summary>
    Task NotifyChoreCreatedAsync(string familyId, ChoreDto choreData, string? triggeredBy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies the family that a chore was updated.
    /// </summary>
    Task NotifyChoreUpdatedAsync(string familyId, ChoreDto choreData, string? triggeredBy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies the family that a chore was deleted.
    /// </summary>
    Task NotifyChoreDeletedAsync(string familyId, string choreId, string? triggeredBy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies the family that a chore was completed.
    /// </summary>
    Task NotifyChoreCompletedAsync(string familyId, string choreId, string completedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies the family that a user's profile was updated.
    /// </summary>
    Task NotifyUserUpdatedAsync(string familyId, string userId, string displayName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies the family that a new user joined.
    /// </summary>
    Task NotifyUserJoinedAsync(string familyId, string userId, string displayName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies the family that a user left.
    /// </summary>
    Task NotifyUserLeftAsync(string familyId, string userId, string displayName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies the family that the family settings were updated.
    /// </summary>
    Task NotifyFamilySettingsUpdatedAsync(string familyId, string? triggeredBy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies the family that a calendar sync completed.
    /// </summary>
    Task NotifyCalendarSyncCompletedAsync(string familyId, CalendarSyncCompletedMessageDto syncResult, CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies the family that a device was linked.
    /// </summary>
    Task NotifyDeviceLinkedAsync(string familyId, string deviceId, string deviceName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies the family that a device was unlinked.
    /// </summary>
    Task NotifyDeviceUnlinkedAsync(string familyId, string deviceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Notifies clients that a full sync is required.
    /// This is used when multiple changes have occurred and it's more efficient to re-fetch all data.
    /// </summary>
    Task NotifyFullSyncRequiredAsync(string familyId, string? reason = null, CancellationToken cancellationToken = default);
}
