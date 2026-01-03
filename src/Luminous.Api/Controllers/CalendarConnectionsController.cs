using Luminous.Application.DTOs;
using Luminous.Application.Features.CalendarConnections.Commands;
using Luminous.Application.Features.CalendarConnections.Queries;
using Luminous.Domain.Enums;
using Luminous.Shared.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Luminous.Api.Controllers;

/// <summary>
/// Controller for managing calendar connections.
/// </summary>
public class CalendarConnectionsController : ApiControllerBase
{
    /// <summary>
    /// Gets all calendar connections for a family.
    /// </summary>
    /// <param name="familyId">The family ID.</param>
    /// <param name="activeOnly">If true, only returns active connections.</param>
    /// <returns>List of calendar connections.</returns>
    [HttpGet("family/{familyId}")]
    [Authorize(Policy = "FamilyMember")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CalendarConnectionSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CalendarConnectionSummaryDto>>>> GetConnections(
        string familyId,
        [FromQuery] bool? activeOnly = null)
    {
        var query = new GetCalendarConnectionsQuery
        {
            FamilyId = familyId,
            ActiveOnly = activeOnly
        };
        var result = await Mediator.Send(query);
        return OkResponse(result);
    }

    /// <summary>
    /// Gets a specific calendar connection.
    /// </summary>
    /// <param name="familyId">The family ID.</param>
    /// <param name="id">The connection ID.</param>
    /// <returns>The calendar connection.</returns>
    [HttpGet("family/{familyId}/{id}")]
    [Authorize(Policy = "FamilyMember")]
    [ProducesResponseType(typeof(ApiResponse<CalendarConnectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CalendarConnectionDto>>> GetConnection(
        string familyId,
        string id)
    {
        var query = new GetCalendarConnectionQuery
        {
            FamilyId = familyId,
            ConnectionId = id
        };
        var result = await Mediator.Send(query);
        return OkResponse(result);
    }

    /// <summary>
    /// Creates a new calendar connection.
    /// </summary>
    /// <param name="familyId">The family ID.</param>
    /// <param name="request">The connection request.</param>
    /// <returns>The created connection.</returns>
    [HttpPost("family/{familyId}")]
    [Authorize(Policy = "FamilyAdmin")]
    [ProducesResponseType(typeof(ApiResponse<CalendarConnectionDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<CalendarConnectionDto>>> CreateConnection(
        string familyId,
        [FromBody] CreateCalendarConnectionRequest request)
    {
        var command = new CreateCalendarConnectionCommand
        {
            FamilyId = familyId,
            Name = request.Name,
            Provider = request.Provider,
            IcsUrl = request.IcsUrl,
            AssignedMemberIds = request.AssignedMemberIds,
            Color = request.Color
        };
        var result = await Mediator.Send(command);
        return CreatedResponse($"/api/calendar-connections/family/{familyId}/{result.Id}", result);
    }

    /// <summary>
    /// Updates a calendar connection.
    /// </summary>
    /// <param name="familyId">The family ID.</param>
    /// <param name="id">The connection ID.</param>
    /// <param name="request">The update request.</param>
    /// <returns>The updated connection.</returns>
    [HttpPut("family/{familyId}/{id}")]
    [Authorize(Policy = "FamilyAdmin")]
    [ProducesResponseType(typeof(ApiResponse<CalendarConnectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CalendarConnectionDto>>> UpdateConnection(
        string familyId,
        string id,
        [FromBody] UpdateCalendarConnectionRequest request)
    {
        var command = new UpdateCalendarConnectionCommand
        {
            FamilyId = familyId,
            ConnectionId = id,
            Name = request.Name,
            AssignedMemberIds = request.AssignedMemberIds,
            Color = request.Color,
            IsEnabled = request.IsEnabled,
            SyncSettings = request.SyncSettings
        };
        var result = await Mediator.Send(command);
        return OkResponse(result);
    }

    /// <summary>
    /// Deletes a calendar connection.
    /// </summary>
    /// <param name="familyId">The family ID.</param>
    /// <param name="id">The connection ID.</param>
    /// <param name="deleteSyncedEvents">Whether to delete synced events (default true).</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("family/{familyId}/{id}")]
    [Authorize(Policy = "FamilyAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteConnection(
        string familyId,
        string id,
        [FromQuery] bool deleteSyncedEvents = true)
    {
        var command = new DeleteCalendarConnectionCommand
        {
            FamilyId = familyId,
            ConnectionId = id,
            DeleteSyncedEvents = deleteSyncedEvents
        };
        await Mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Starts the OAuth flow for a calendar connection.
    /// </summary>
    /// <param name="familyId">The family ID.</param>
    /// <param name="id">The connection ID.</param>
    /// <param name="request">The OAuth start request.</param>
    /// <returns>The OAuth authorization URL.</returns>
    [HttpPost("family/{familyId}/{id}/oauth/start")]
    [Authorize(Policy = "FamilyAdmin")]
    [ProducesResponseType(typeof(ApiResponse<CalendarOAuthStartDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CalendarOAuthStartDto>>> StartOAuth(
        string familyId,
        string id,
        [FromBody] StartOAuthRequest request)
    {
        var command = new StartOAuthCommand
        {
            FamilyId = familyId,
            ConnectionId = id,
            RedirectUri = request.RedirectUri
        };
        var result = await Mediator.Send(command);
        return OkResponse(result);
    }

    /// <summary>
    /// Completes the OAuth flow for a calendar connection.
    /// </summary>
    /// <param name="familyId">The family ID.</param>
    /// <param name="id">The connection ID.</param>
    /// <param name="request">The OAuth completion request.</param>
    /// <returns>The updated connection.</returns>
    [HttpPost("family/{familyId}/{id}/oauth/complete")]
    [Authorize(Policy = "FamilyAdmin")]
    [ProducesResponseType(typeof(ApiResponse<CalendarConnectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CalendarConnectionDto>>> CompleteOAuth(
        string familyId,
        string id,
        [FromBody] CompleteOAuthRequest request)
    {
        var command = new CompleteOAuthCommand
        {
            ConnectionId = id,
            FamilyId = familyId,
            Code = request.Code,
            RedirectUri = request.RedirectUri
        };
        var result = await Mediator.Send(command);
        return OkResponse(result);
    }

    /// <summary>
    /// Manually triggers a sync for a calendar connection.
    /// </summary>
    /// <param name="familyId">The family ID.</param>
    /// <param name="id">The connection ID.</param>
    /// <returns>The sync result.</returns>
    [HttpPost("family/{familyId}/{id}/sync")]
    [Authorize(Policy = "FamilyAdmin")]
    [ProducesResponseType(typeof(ApiResponse<CalendarSyncResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CalendarSyncResultDto>>> SyncConnection(
        string familyId,
        string id)
    {
        var command = new SyncCalendarConnectionCommand
        {
            FamilyId = familyId,
            ConnectionId = id
        };
        var result = await Mediator.Send(command);
        return OkResponse(result);
    }
}

/// <summary>
/// Request to create a calendar connection.
/// </summary>
public record CreateCalendarConnectionRequest
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
/// Request to update a calendar connection.
/// </summary>
public record UpdateCalendarConnectionRequest
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
/// Request to start OAuth flow.
/// </summary>
public record StartOAuthRequest
{
    /// <summary>
    /// The OAuth redirect URI.
    /// </summary>
    public required string RedirectUri { get; init; }
}

/// <summary>
/// Request to complete OAuth flow.
/// </summary>
public record CompleteOAuthRequest
{
    /// <summary>
    /// The authorization code from the OAuth provider.
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// The OAuth redirect URI (must match the one used in start).
    /// </summary>
    public required string RedirectUri { get; init; }
}
