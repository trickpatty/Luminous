using Luminous.Application.DTOs;
using Luminous.Application.Features.Chores.Queries;
using Luminous.Shared.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Luminous.Api.Controllers;

/// <summary>
/// Controller for chore management.
/// </summary>
[Authorize]
public class ChoresController : ApiControllerBase
{
    /// <summary>
    /// Gets chores for a family.
    /// </summary>
    /// <param name="familyId">The family ID.</param>
    /// <param name="assigneeId">Optional assignee filter.</param>
    /// <param name="includeInactive">Whether to include inactive chores.</param>
    /// <returns>The list of chores.</returns>
    [HttpGet("family/{familyId}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ChoreDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ChoreDto>>>> GetChores(
        string familyId,
        [FromQuery] string? assigneeId = null,
        [FromQuery] bool includeInactive = false)
    {
        var query = new GetChoresQuery
        {
            FamilyId = familyId,
            AssigneeId = assigneeId,
            IncludeInactive = includeInactive
        };
        var result = await Mediator.Send(query);
        return OkResponse(result);
    }
}
