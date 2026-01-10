using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Luminous.Api.Hubs;

/// <summary>
/// SignalR hub for real-time family synchronization.
/// Provides family-scoped message groups for push updates to connected clients.
/// </summary>
[Authorize]
public class SyncHub : Hub
{
    private readonly ILogger<SyncHub> _logger;

    public SyncHub(ILogger<SyncHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Called when a client connects to the hub.
    /// Automatically adds the client to their family's group.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var familyId = GetFamilyId();
        var userId = GetUserId();

        if (!string.IsNullOrEmpty(familyId))
        {
            var groupName = GetFamilyGroupName(familyId);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            _logger.LogInformation(
                "Client connected: ConnectionId={ConnectionId}, UserId={UserId}, FamilyId={FamilyId}",
                Context.ConnectionId, userId, familyId);
        }
        else
        {
            _logger.LogWarning(
                "Client connected without family_id claim: ConnectionId={ConnectionId}",
                Context.ConnectionId);
        }

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects from the hub.
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var familyId = GetFamilyId();
        var userId = GetUserId();

        if (!string.IsNullOrEmpty(familyId))
        {
            var groupName = GetFamilyGroupName(familyId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            _logger.LogInformation(
                "Client disconnected: ConnectionId={ConnectionId}, UserId={UserId}, FamilyId={FamilyId}",
                Context.ConnectionId, userId, familyId);
        }

        if (exception != null)
        {
            _logger.LogError(exception,
                "Client disconnected with error: ConnectionId={ConnectionId}",
                Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Allows a client to explicitly join their family group (useful for reconnection).
    /// </summary>
    public async Task JoinFamilyGroup()
    {
        var familyId = GetFamilyId();
        if (string.IsNullOrEmpty(familyId))
        {
            throw new HubException("User is not associated with a family.");
        }

        var groupName = GetFamilyGroupName(familyId);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        _logger.LogDebug(
            "Client joined family group: ConnectionId={ConnectionId}, FamilyId={FamilyId}",
            Context.ConnectionId, familyId);
    }

    /// <summary>
    /// Allows a client to leave their family group.
    /// </summary>
    public async Task LeaveFamilyGroup()
    {
        var familyId = GetFamilyId();
        if (string.IsNullOrEmpty(familyId))
        {
            return;
        }

        var groupName = GetFamilyGroupName(familyId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

        _logger.LogDebug(
            "Client left family group: ConnectionId={ConnectionId}, FamilyId={FamilyId}",
            Context.ConnectionId, familyId);
    }

    /// <summary>
    /// Sends a ping to verify connection is active.
    /// Returns a pong response.
    /// </summary>
    public Task<string> Ping()
    {
        return Task.FromResult("pong");
    }

    private string? GetFamilyId()
    {
        return Context.User?.FindFirst("family_id")?.Value;
    }

    private string? GetUserId()
    {
        return Context.User?.FindFirst("sub")?.Value
            ?? Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    }

    /// <summary>
    /// Gets the SignalR group name for a family.
    /// </summary>
    internal static string GetFamilyGroupName(string familyId) => $"family:{familyId}";
}
