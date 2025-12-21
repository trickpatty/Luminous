using Luminous.Application.DTOs;
using Luminous.Application.Features.Users.Queries;
using Luminous.Shared.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Luminous.Api.Controllers;

/// <summary>
/// Controller for user management.
/// </summary>
[Authorize]
public class UsersController : ApiControllerBase
{
    /// <summary>
    /// Gets all members of a family.
    /// </summary>
    /// <param name="familyId">The family ID.</param>
    /// <returns>The list of family members.</returns>
    [HttpGet("family/{familyId}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<UserDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<UserDto>>>> GetFamilyMembers(string familyId)
    {
        var result = await Mediator.Send(new GetFamilyMembersQuery { FamilyId = familyId });
        return OkResponse(result);
    }
}
