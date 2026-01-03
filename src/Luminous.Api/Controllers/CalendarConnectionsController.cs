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

    // =========================================================================
    // NEW SESSION-BASED OAUTH ENDPOINTS
    // =========================================================================

    /// <summary>
    /// Initiates an OAuth flow to connect calendars from a provider.
    /// Returns a session ID and authorization URL.
    /// </summary>
    /// <param name="familyId">The family ID.</param>
    /// <param name="request">The OAuth initiation request.</param>
    /// <returns>Session ID, authorization URL, and state.</returns>
    [HttpPost("family/{familyId}/oauth/start")]
    [Authorize(Policy = "FamilyAdmin")]
    [ProducesResponseType(typeof(ApiResponse<OAuthInitiateResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<OAuthInitiateResponse>>> InitiateOAuth(
        string familyId,
        [FromBody] InitiateOAuthRequest request)
    {
        var command = new InitiateOAuthCommand
        {
            FamilyId = familyId,
            Provider = request.Provider,
            RedirectUri = request.RedirectUri
        };
        var result = await Mediator.Send(command);
        return OkResponse(result);
    }

    /// <summary>
    /// Completes an OAuth flow and returns the list of available calendars.
    /// </summary>
    /// <param name="familyId">The family ID.</param>
    /// <param name="request">The OAuth completion request.</param>
    /// <returns>Session ID, account email, and available calendars.</returns>
    [HttpPost("family/{familyId}/oauth/complete")]
    [Authorize(Policy = "FamilyAdmin")]
    [ProducesResponseType(typeof(ApiResponse<OAuthCompleteResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<OAuthCompleteResponse>>> CompleteOAuthSession(
        string familyId,
        [FromBody] CompleteOAuthSessionRequest request)
    {
        var command = new CompleteOAuthSessionCommand
        {
            FamilyId = familyId,
            Code = request.Code,
            State = request.State,
            RedirectUri = request.RedirectUri
        };
        var result = await Mediator.Send(command);
        return OkResponse(result);
    }

    /// <summary>
    /// Creates calendar connections from a completed OAuth session.
    /// </summary>
    /// <param name="familyId">The family ID.</param>
    /// <param name="request">The connection creation request.</param>
    /// <returns>The created connections.</returns>
    [HttpPost("family/{familyId}/oauth/connections")]
    [Authorize(Policy = "FamilyAdmin")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CalendarConnectionDto>>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CalendarConnectionDto>>>> CreateConnectionsFromSession(
        string familyId,
        [FromBody] CreateConnectionsFromSessionRequest request)
    {
        var command = new CreateConnectionsFromSessionCommand
        {
            FamilyId = familyId,
            SessionId = request.SessionId,
            Calendars = request.Calendars.Select(c => new CreateConnectionFromSessionRequest
            {
                ExternalCalendarId = c.ExternalCalendarId,
                DisplayName = c.DisplayName,
                Color = c.Color,
                AssignedMemberIds = c.AssignedMemberIds
            }).ToList()
        };
        var result = await Mediator.Send(command);
        return CreatedResponse($"/api/calendar-connections/family/{familyId}", result);
    }

    /// <summary>
    /// Validates an ICS URL before creating a connection.
    /// </summary>
    /// <param name="familyId">The family ID.</param>
    /// <param name="request">The validation request.</param>
    /// <returns>Validation result with calendar info if valid.</returns>
    [HttpPost("family/{familyId}/validate-ics")]
    [Authorize(Policy = "FamilyAdmin")]
    [ProducesResponseType(typeof(ApiResponse<ValidateIcsUrlResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ValidateIcsUrlResponse>>> ValidateIcsUrl(
        string familyId,
        [FromBody] ValidateIcsUrlRequest request)
    {
        var command = new ValidateIcsUrlCommand
        {
            FamilyId = familyId,
            Url = request.Url
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

// =========================================================================
// NEW SESSION-BASED OAUTH REQUEST DTOS
// =========================================================================

/// <summary>
/// Request to initiate OAuth flow.
/// </summary>
public record InitiateOAuthRequest
{
    /// <summary>
    /// The calendar provider to connect.
    /// </summary>
    public required CalendarProvider Provider { get; init; }

    /// <summary>
    /// The OAuth redirect URI.
    /// </summary>
    public required string RedirectUri { get; init; }
}

/// <summary>
/// Request to complete OAuth session.
/// </summary>
public record CompleteOAuthSessionRequest
{
    /// <summary>
    /// The authorization code from the OAuth provider.
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// The state parameter from the OAuth callback.
    /// </summary>
    public required string State { get; init; }

    /// <summary>
    /// The OAuth redirect URI (must match the one used in start).
    /// </summary>
    public required string RedirectUri { get; init; }
}

/// <summary>
/// Request to create connections from an OAuth session.
/// </summary>
public record CreateConnectionsFromSessionRequest
{
    /// <summary>
    /// The OAuth session ID.
    /// </summary>
    public required string SessionId { get; init; }

    /// <summary>
    /// The calendars to connect.
    /// </summary>
    public required List<CreateConnectionFromSessionCalendarRequest> Calendars { get; init; }
}

/// <summary>
/// Calendar to connect from an OAuth session.
/// </summary>
public record CreateConnectionFromSessionCalendarRequest
{
    /// <summary>
    /// External calendar ID from the provider.
    /// </summary>
    public required string ExternalCalendarId { get; init; }

    /// <summary>
    /// Display name for the calendar.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Calendar color (hex code).
    /// </summary>
    public string? Color { get; init; }

    /// <summary>
    /// Family member IDs to assign this calendar to.
    /// </summary>
    public List<string> AssignedMemberIds { get; init; } = [];
}

/// <summary>
/// Request to validate an ICS URL.
/// </summary>
public record ValidateIcsUrlRequest
{
    /// <summary>
    /// The ICS URL to validate.
    /// </summary>
    public required string Url { get; init; }
}
