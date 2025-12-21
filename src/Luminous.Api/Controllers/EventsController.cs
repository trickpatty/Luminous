using Luminous.Application.DTOs;
using Luminous.Application.Features.Events.Queries;
using Luminous.Shared.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Luminous.Api.Controllers;

/// <summary>
/// Controller for calendar event management.
/// </summary>
[Authorize]
public class EventsController : ApiControllerBase
{
    /// <summary>
    /// Gets events for a family within a date range.
    /// </summary>
    /// <param name="familyId">The family ID.</param>
    /// <param name="startDate">The start date.</param>
    /// <param name="endDate">The end date.</param>
    /// <param name="assigneeId">Optional assignee filter.</param>
    /// <returns>The list of events.</returns>
    [HttpGet("family/{familyId}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<EventSummaryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<EventSummaryDto>>>> GetEvents(
        string familyId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] string? assigneeId = null)
    {
        var query = new GetEventsQuery
        {
            FamilyId = familyId,
            StartDate = startDate,
            EndDate = endDate,
            AssigneeId = assigneeId
        };
        var result = await Mediator.Send(query);
        return OkResponse(result);
    }
}
