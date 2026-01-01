using Luminous.Application.DTOs;
using Luminous.Application.Features.Users.Commands;
using Luminous.Application.Features.Users.Queries;
using Luminous.Domain.Enums;
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
    /// <param name="request">The updated profile information.</param>
    /// <returns>The updated user.</returns>
    [HttpPut("family/{familyId}/{userId}/profile")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateProfile(
        string familyId,
        string userId,
        [FromBody] UpdateUserProfileRequestDto request)
    {
        var command = new UpdateUserProfileCommand
        {
            FamilyId = familyId,
            UserId = userId,
            DisplayName = request.DisplayName,
            Profile = request.Profile
        };
        var result = await Mediator.Send(command);
        return OkResponse(result);
    }

    /// <summary>
    /// Updates a user's role within the family.
    /// </summary>
    /// <param name="familyId">The family ID.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="request">The role update request.</param>
    /// <returns>The updated user.</returns>
    [HttpPut("family/{familyId}/{userId}/role")]
    [Authorize(Policy = "FamilyAdmin")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateRole(
        string familyId,
        string userId,
        [FromBody] UpdateUserRoleRequestDto request)
    {
        var command = new UpdateUserRoleCommand
        {
            FamilyId = familyId,
            UserId = userId,
            NewRole = request.NewRole
        };
        var result = await Mediator.Send(command);
        return OkResponse(result);
    }

    /// <summary>
    /// Updates a user's caregiver information.
    /// </summary>
    /// <param name="familyId">The family ID.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="request">The caregiver info update request.</param>
    /// <returns>The updated user.</returns>
    [HttpPut("family/{familyId}/{userId}/caregiver-info")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateCaregiverInfo(
        string familyId,
        string userId,
        [FromBody] UpdateCaregiverInfoRequestDto request)
    {
        var command = new UpdateCaregiverInfoCommand
        {
            FamilyId = familyId,
            UserId = userId,
            Allergies = request.Allergies,
            MedicalNotes = request.MedicalNotes,
            EmergencyContactName = request.EmergencyContactName,
            EmergencyContactPhone = request.EmergencyContactPhone,
            DoctorName = request.DoctorName,
            DoctorPhone = request.DoctorPhone,
            SchoolName = request.SchoolName,
            Notes = request.Notes
        };
        var result = await Mediator.Send(command);
        return OkResponse(result);
    }

    /// <summary>
    /// Removes a user from the family.
    /// </summary>
    /// <param name="familyId">The family ID.</param>
    /// <param name="userId">The user ID.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("family/{familyId}/{userId}")]
    [Authorize(Policy = "FamilyAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveUser(string familyId, string userId)
    {
        var command = new RemoveUserFromFamilyCommand
        {
            FamilyId = familyId,
            UserId = userId
        };
        await Mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Generates a time-limited caregiver access token for viewing a user's information.
    /// </summary>
    /// <param name="familyId">The family ID.</param>
    /// <param name="userId">The user ID whose info will be shared.</param>
    /// <param name="request">The token generation request.</param>
    /// <returns>The caregiver access token with URL.</returns>
    [HttpPost("family/{familyId}/{userId}/caregiver-token")]
    [Authorize(Policy = "FamilyAdmin")]
    [ProducesResponseType(typeof(ApiResponse<CaregiverAccessTokenDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CaregiverAccessTokenDto>>> GenerateCaregiverToken(
        string familyId,
        string userId,
        [FromBody] GenerateCaregiverTokenRequestDto request)
    {
        var command = new GenerateCaregiverAccessTokenCommand
        {
            FamilyId = familyId,
            UserId = userId,
            ExpirationHours = request.ExpirationHours
        };
        var result = await Mediator.Send(command);
        return OkResponse(result);
    }
}
