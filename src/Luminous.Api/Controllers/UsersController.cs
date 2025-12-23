using Luminous.Application.DTOs;
using Luminous.Application.Features.Users.Commands;
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
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<UserDto>>>> GetFamilyMembers(string familyId)
    {
        var result = await Mediator.Send(new GetFamilyMembersQuery { FamilyId = familyId });
        return OkResponse(result);
    }

    /// <summary>
    /// Gets a specific user by ID.
    /// </summary>
    /// <param name="familyId">The family ID.</param>
    /// <param name="userId">The user ID.</param>
    /// <returns>The user details.</returns>
    [HttpGet("family/{familyId}/{userId}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUser(string familyId, string userId)
    {
        var result = await Mediator.Send(new GetUserQuery { FamilyId = familyId, UserId = userId });
        return OkResponse(result);
    }

    /// <summary>
    /// Updates a user's profile.
    /// </summary>
    /// <param name="familyId">The family ID.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="profile">The updated profile information.</param>
    /// <returns>The updated user.</returns>
    [HttpPut("family/{familyId}/{userId}/profile")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateProfile(
        string familyId,
        string userId,
        [FromBody] UserProfileDto profile)
    {
        var command = new UpdateUserProfileCommand
        {
            FamilyId = familyId,
            UserId = userId,
            Profile = profile
        };
        var result = await Mediator.Send(command);
        return OkResponse(result);
    }
}
