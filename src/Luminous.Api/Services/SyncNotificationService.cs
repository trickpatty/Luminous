using Luminous.Api.Hubs;
using Luminous.Application.Common.Interfaces;
using Luminous.Application.DTOs;
using Microsoft.AspNetCore.SignalR;

namespace Luminous.Api.Services;

/// <summary>
/// Service for sending real-time sync notifications via SignalR.
/// </summary>
public class SyncNotificationService : ISyncNotificationService
{
    private readonly IHubContext<SyncHub> _hubContext;
    private readonly ILogger<SyncNotificationService> _logger;

    // SignalR method names (must match client expectations)
    private const string MethodSync = "Sync";
    private const string MethodEventChanged = "EventChanged";
    private const string MethodChoreChanged = "ChoreChanged";
    private const string MethodUserChanged = "UserChanged";
    private const string MethodFamilyChanged = "FamilyChanged";
    private const string MethodCalendarSyncCompleted = "CalendarSyncCompleted";
    private const string MethodDeviceChanged = "DeviceChanged";
    private const string MethodFullSyncRequired = "FullSyncRequired";

    public SyncNotificationService(
        IHubContext<SyncHub> hubContext,
        ILogger<SyncNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendToFamilyAsync(string familyId, SyncMessageDto message, CancellationToken cancellationToken = default)
    {
        var groupName = SyncHub.GetFamilyGroupName(familyId);

        try
        {
            await _hubContext.Clients
                .Group(groupName)
                .SendAsync(MethodSync, message, cancellationToken);

            _logger.LogDebug(
                "Sent sync message: Type={MessageType}, FamilyId={FamilyId}, EntityId={EntityId}",
                message.Type, familyId, message.EntityId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send sync message: Type={MessageType}, FamilyId={FamilyId}",
                message.Type, familyId);
        }
    }

    public async Task NotifyEventCreatedAsync(string familyId, EventSummaryDto eventData, string? triggeredBy = null, CancellationToken cancellationToken = default)
    {
        var groupName = SyncHub.GetFamilyGroupName(familyId);
        var message = new EventSyncMessageDto
        {
            Type = SyncMessageType.EventCreated,
            EventId = eventData.Id,
            Event = eventData,
            Timestamp = DateTime.UtcNow
        };

        try
        {
            await _hubContext.Clients
                .Group(groupName)
                .SendAsync(MethodEventChanged, message, cancellationToken);

            _logger.LogDebug("Notified EventCreated: FamilyId={FamilyId}, EventId={EventId}", familyId, eventData.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify EventCreated: FamilyId={FamilyId}, EventId={EventId}", familyId, eventData.Id);
        }
    }

    public async Task NotifyEventUpdatedAsync(string familyId, EventSummaryDto eventData, string? triggeredBy = null, CancellationToken cancellationToken = default)
    {
        var groupName = SyncHub.GetFamilyGroupName(familyId);
        var message = new EventSyncMessageDto
        {
            Type = SyncMessageType.EventUpdated,
            EventId = eventData.Id,
            Event = eventData,
            Timestamp = DateTime.UtcNow
        };

        try
        {
            await _hubContext.Clients
                .Group(groupName)
                .SendAsync(MethodEventChanged, message, cancellationToken);

            _logger.LogDebug("Notified EventUpdated: FamilyId={FamilyId}, EventId={EventId}", familyId, eventData.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify EventUpdated: FamilyId={FamilyId}, EventId={EventId}", familyId, eventData.Id);
        }
    }

    public async Task NotifyEventDeletedAsync(string familyId, string eventId, string? triggeredBy = null, CancellationToken cancellationToken = default)
    {
        var groupName = SyncHub.GetFamilyGroupName(familyId);
        var message = new EventSyncMessageDto
        {
            Type = SyncMessageType.EventDeleted,
            EventId = eventId,
            Event = null,
            Timestamp = DateTime.UtcNow
        };

        try
        {
            await _hubContext.Clients
                .Group(groupName)
                .SendAsync(MethodEventChanged, message, cancellationToken);

            _logger.LogDebug("Notified EventDeleted: FamilyId={FamilyId}, EventId={EventId}", familyId, eventId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify EventDeleted: FamilyId={FamilyId}, EventId={EventId}", familyId, eventId);
        }
    }

    public async Task NotifyEventsRefreshedAsync(string familyId, string? triggeredBy = null, CancellationToken cancellationToken = default)
    {
        var groupName = SyncHub.GetFamilyGroupName(familyId);
        var message = new SyncMessageDto
        {
            Type = SyncMessageType.EventsRefreshed,
            FamilyId = familyId,
            TriggeredBy = triggeredBy,
            Timestamp = DateTime.UtcNow
        };

        try
        {
            await _hubContext.Clients
                .Group(groupName)
                .SendAsync(MethodSync, message, cancellationToken);

            _logger.LogDebug("Notified EventsRefreshed: FamilyId={FamilyId}", familyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify EventsRefreshed: FamilyId={FamilyId}", familyId);
        }
    }

    public async Task NotifyChoreCreatedAsync(string familyId, ChoreDto choreData, string? triggeredBy = null, CancellationToken cancellationToken = default)
    {
        var groupName = SyncHub.GetFamilyGroupName(familyId);
        var message = new ChoreSyncMessageDto
        {
            Type = SyncMessageType.ChoreCreated,
            ChoreId = choreData.Id,
            Chore = choreData,
            Timestamp = DateTime.UtcNow
        };

        try
        {
            await _hubContext.Clients
                .Group(groupName)
                .SendAsync(MethodChoreChanged, message, cancellationToken);

            _logger.LogDebug("Notified ChoreCreated: FamilyId={FamilyId}, ChoreId={ChoreId}", familyId, choreData.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify ChoreCreated: FamilyId={FamilyId}, ChoreId={ChoreId}", familyId, choreData.Id);
        }
    }

    public async Task NotifyChoreUpdatedAsync(string familyId, ChoreDto choreData, string? triggeredBy = null, CancellationToken cancellationToken = default)
    {
        var groupName = SyncHub.GetFamilyGroupName(familyId);
        var message = new ChoreSyncMessageDto
        {
            Type = SyncMessageType.ChoreUpdated,
            ChoreId = choreData.Id,
            Chore = choreData,
            Timestamp = DateTime.UtcNow
        };

        try
        {
            await _hubContext.Clients
                .Group(groupName)
                .SendAsync(MethodChoreChanged, message, cancellationToken);

            _logger.LogDebug("Notified ChoreUpdated: FamilyId={FamilyId}, ChoreId={ChoreId}", familyId, choreData.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify ChoreUpdated: FamilyId={FamilyId}, ChoreId={ChoreId}", familyId, choreData.Id);
        }
    }

    public async Task NotifyChoreDeletedAsync(string familyId, string choreId, string? triggeredBy = null, CancellationToken cancellationToken = default)
    {
        var groupName = SyncHub.GetFamilyGroupName(familyId);
        var message = new ChoreSyncMessageDto
        {
            Type = SyncMessageType.ChoreDeleted,
            ChoreId = choreId,
            Chore = null,
            Timestamp = DateTime.UtcNow
        };

        try
        {
            await _hubContext.Clients
                .Group(groupName)
                .SendAsync(MethodChoreChanged, message, cancellationToken);

            _logger.LogDebug("Notified ChoreDeleted: FamilyId={FamilyId}, ChoreId={ChoreId}", familyId, choreId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify ChoreDeleted: FamilyId={FamilyId}, ChoreId={ChoreId}", familyId, choreId);
        }
    }

    public async Task NotifyChoreCompletedAsync(string familyId, string choreId, string completedBy, CancellationToken cancellationToken = default)
    {
        var groupName = SyncHub.GetFamilyGroupName(familyId);
        var message = new ChoreSyncMessageDto
        {
            Type = SyncMessageType.ChoreCompleted,
            ChoreId = choreId,
            CompletedBy = completedBy,
            Timestamp = DateTime.UtcNow
        };

        try
        {
            await _hubContext.Clients
                .Group(groupName)
                .SendAsync(MethodChoreChanged, message, cancellationToken);

            _logger.LogDebug("Notified ChoreCompleted: FamilyId={FamilyId}, ChoreId={ChoreId}, CompletedBy={CompletedBy}", familyId, choreId, completedBy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify ChoreCompleted: FamilyId={FamilyId}, ChoreId={ChoreId}", familyId, choreId);
        }
    }

    public async Task NotifyUserUpdatedAsync(string familyId, string userId, string displayName, CancellationToken cancellationToken = default)
    {
        var groupName = SyncHub.GetFamilyGroupName(familyId);
        var message = new UserSyncMessageDto
        {
            Type = SyncMessageType.UserUpdated,
            UserId = userId,
            DisplayName = displayName,
            Timestamp = DateTime.UtcNow
        };

        try
        {
            await _hubContext.Clients
                .Group(groupName)
                .SendAsync(MethodUserChanged, message, cancellationToken);

            _logger.LogDebug("Notified UserUpdated: FamilyId={FamilyId}, UserId={UserId}", familyId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify UserUpdated: FamilyId={FamilyId}, UserId={UserId}", familyId, userId);
        }
    }

    public async Task NotifyUserJoinedAsync(string familyId, string userId, string displayName, CancellationToken cancellationToken = default)
    {
        var groupName = SyncHub.GetFamilyGroupName(familyId);
        var message = new UserSyncMessageDto
        {
            Type = SyncMessageType.UserJoined,
            UserId = userId,
            DisplayName = displayName,
            Timestamp = DateTime.UtcNow
        };

        try
        {
            await _hubContext.Clients
                .Group(groupName)
                .SendAsync(MethodUserChanged, message, cancellationToken);

            _logger.LogDebug("Notified UserJoined: FamilyId={FamilyId}, UserId={UserId}", familyId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify UserJoined: FamilyId={FamilyId}, UserId={UserId}", familyId, userId);
        }
    }

    public async Task NotifyUserLeftAsync(string familyId, string userId, string displayName, CancellationToken cancellationToken = default)
    {
        var groupName = SyncHub.GetFamilyGroupName(familyId);
        var message = new UserSyncMessageDto
        {
            Type = SyncMessageType.UserLeft,
            UserId = userId,
            DisplayName = displayName,
            Timestamp = DateTime.UtcNow
        };

        try
        {
            await _hubContext.Clients
                .Group(groupName)
                .SendAsync(MethodUserChanged, message, cancellationToken);

            _logger.LogDebug("Notified UserLeft: FamilyId={FamilyId}, UserId={UserId}", familyId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify UserLeft: FamilyId={FamilyId}, UserId={UserId}", familyId, userId);
        }
    }

    public async Task NotifyFamilySettingsUpdatedAsync(string familyId, string? triggeredBy = null, CancellationToken cancellationToken = default)
    {
        var groupName = SyncHub.GetFamilyGroupName(familyId);
        var message = new SyncMessageDto
        {
            Type = SyncMessageType.FamilySettingsUpdated,
            FamilyId = familyId,
            TriggeredBy = triggeredBy,
            Timestamp = DateTime.UtcNow
        };

        try
        {
            await _hubContext.Clients
                .Group(groupName)
                .SendAsync(MethodFamilyChanged, message, cancellationToken);

            _logger.LogDebug("Notified FamilySettingsUpdated: FamilyId={FamilyId}", familyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify FamilySettingsUpdated: FamilyId={FamilyId}", familyId);
        }
    }

    public async Task NotifyCalendarSyncCompletedAsync(string familyId, CalendarSyncCompletedMessageDto syncResult, CancellationToken cancellationToken = default)
    {
        var groupName = SyncHub.GetFamilyGroupName(familyId);

        try
        {
            await _hubContext.Clients
                .Group(groupName)
                .SendAsync(MethodCalendarSyncCompleted, syncResult, cancellationToken);

            _logger.LogDebug(
                "Notified CalendarSyncCompleted: FamilyId={FamilyId}, ConnectionId={ConnectionId}, Added={Added}, Updated={Updated}, Removed={Removed}",
                familyId, syncResult.CalendarConnectionId, syncResult.EventsAdded, syncResult.EventsUpdated, syncResult.EventsRemoved);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify CalendarSyncCompleted: FamilyId={FamilyId}", familyId);
        }
    }

    public async Task NotifyDeviceLinkedAsync(string familyId, string deviceId, string deviceName, CancellationToken cancellationToken = default)
    {
        var groupName = SyncHub.GetFamilyGroupName(familyId);
        var message = new SyncMessageDto
        {
            Type = SyncMessageType.DeviceLinked,
            FamilyId = familyId,
            EntityId = deviceId,
            Payload = new { DeviceId = deviceId, DeviceName = deviceName },
            Timestamp = DateTime.UtcNow
        };

        try
        {
            await _hubContext.Clients
                .Group(groupName)
                .SendAsync(MethodDeviceChanged, message, cancellationToken);

            _logger.LogDebug("Notified DeviceLinked: FamilyId={FamilyId}, DeviceId={DeviceId}", familyId, deviceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify DeviceLinked: FamilyId={FamilyId}, DeviceId={DeviceId}", familyId, deviceId);
        }
    }

    public async Task NotifyDeviceUnlinkedAsync(string familyId, string deviceId, CancellationToken cancellationToken = default)
    {
        var groupName = SyncHub.GetFamilyGroupName(familyId);
        var message = new SyncMessageDto
        {
            Type = SyncMessageType.DeviceUnlinked,
            FamilyId = familyId,
            EntityId = deviceId,
            Timestamp = DateTime.UtcNow
        };

        try
        {
            await _hubContext.Clients
                .Group(groupName)
                .SendAsync(MethodDeviceChanged, message, cancellationToken);

            _logger.LogDebug("Notified DeviceUnlinked: FamilyId={FamilyId}, DeviceId={DeviceId}", familyId, deviceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify DeviceUnlinked: FamilyId={FamilyId}, DeviceId={DeviceId}", familyId, deviceId);
        }
    }

    public async Task NotifyFullSyncRequiredAsync(string familyId, string? reason = null, CancellationToken cancellationToken = default)
    {
        var groupName = SyncHub.GetFamilyGroupName(familyId);
        var message = new SyncMessageDto
        {
            Type = SyncMessageType.FullSyncRequired,
            FamilyId = familyId,
            Payload = reason != null ? new { Reason = reason } : null,
            Timestamp = DateTime.UtcNow
        };

        try
        {
            await _hubContext.Clients
                .Group(groupName)
                .SendAsync(MethodFullSyncRequired, message, cancellationToken);

            _logger.LogInformation("Notified FullSyncRequired: FamilyId={FamilyId}, Reason={Reason}", familyId, reason);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify FullSyncRequired: FamilyId={FamilyId}", familyId);
        }
    }
}
