using Luminous.Application.DTOs;
using Luminous.Application.Features.Invitations.Commands;
using Luminous.Application.Features.Invitations.Queries;
using Luminous.Domain.Enums;
using Luminous.Shared.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Luminous.Api.Controllers;

/// <summary>
/// Controller for family invitation management.
/// </summary>
public class InvitationsController : ApiControllerBase
{
    /// <summary>
    /// Gets an invitation by its code.
    /// This endpoint is public so invitees can view their invitation details.
    /// </summary>
    /// <param name="code">The invitation code.</param>
    /// <returns>The invitation details.</returns>
    [HttpGet("{code}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<InvitationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<InvitationDto>>> GetInvitationByCode(string code)
    {
        var query = new GetInvitationByCodeQuery { Code = code };
        var result = await Mediator.Send(query);
        return OkResponse(result);
    }

    /// <summary>
    /// Accepts an invitation and creates the user account.
    /// </summary>
    /// <param name="code">The invitation code.</param>
    /// <param name="request">The accept request with user details.</param>
    /// <returns>The created user with authentication token.</returns>
    [HttpPost("{code}/accept")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AcceptedInvitationResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<AcceptedInvitationResultDto>>> AcceptInvitation(
        string code,
        [FromBody] AcceptInvitationRequestDto request)
    {
        var command = new AcceptInvitationCommand
        {
            Code = code,
            DisplayName = request.DisplayName,
            Nickname = request.Nickname,
            AvatarUrl = request.AvatarUrl,
            Color = request.Color
        };
        var result = await Mediator.Send(command);
        return OkResponse(result);
    }

    /// <summary>
    /// Declines an invitation.
    /// </summary>
    /// <param name="code">The invitation code.</param>
    /// <returns>No content on success.</returns>
    [HttpPost("{code}/decline")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeclineInvitation(string code)
    {
        var command = new DeclineInvitationCommand { Code = code };
        await Mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Gets all invitations for a family.
    /// </summary>
    /// <param name="familyId">The family ID.</param>
    /// <param name="status">Optional status filter.</param>
    /// <returns>The list of invitations.</returns>
    [HttpGet("family/{familyId}")]
    [Authorize(Policy = "FamilyAdmin")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<InvitationDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<InvitationDto>>>> GetFamilyInvitations(
        string familyId,
        [FromQuery] InvitationStatus? status = null)
    {
        var query = new GetFamilyInvitationsQuery
        {
            FamilyId = familyId,
            Status = status
        };
        var result = await Mediator.Send(query);
        return OkResponse(result);
    }

    /// <summary>
    /// Gets pending invitations for a family.
    /// </summary>
    /// <param name="familyId">The family ID.</param>
    /// <returns>The list of pending invitations.</returns>
    [HttpGet("family/{familyId}/pending")]
    [Authorize(Policy = "FamilyAdmin")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<InvitationDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<InvitationDto>>>> GetPendingInvitations(string familyId)
    {
        var query = new GetPendingInvitationsQuery { FamilyId = familyId };
        var result = await Mediator.Send(query);
        return OkResponse(result);
    }

    /// <summary>
    /// Creates and sends a new invitation.
    /// </summary>
    /// <param name="familyId">The family ID.</param>
    /// <param name="request">The invitation request.</param>
    /// <returns>The created invitation.</returns>
    [HttpPost("family/{familyId}")]
    [Authorize(Policy = "FamilyAdmin")]
    [ProducesResponseType(typeof(ApiResponse<InvitationDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<InvitationDto>>> CreateInvitation(
        string familyId,
        [FromBody] SendInvitationRequestDto request)
    {
        var command = new CreateInvitationCommand
        {
            FamilyId = familyId,
            Email = request.Email,
            Role = request.Role,
            Message = request.Message
        };
        var result = await Mediator.Send(command);
        return CreatedResponse($"/api/invitations/{result.Code}", result);
    }

    /// <summary>
    /// Revokes a pending invitation.
    /// </summary>
    /// <param name="familyId">The family ID.</param>
    /// <param name="id">The invitation ID.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("family/{familyId}/{id}")]
    [Authorize(Policy = "FamilyAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeInvitation(string familyId, string id)
    {
        var command = new RevokeInvitationCommand
        {
            FamilyId = familyId,
            InvitationId = id
        };
        await Mediator.Send(command);
        return NoContent();
    }
}
