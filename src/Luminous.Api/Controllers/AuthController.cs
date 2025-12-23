using Luminous.Application.DTOs;
using Luminous.Application.Features.Auth.Commands;
using Luminous.Application.Features.Users.Queries;
using Luminous.Shared.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Luminous.Api.Controllers;

/// <summary>
/// Controller for authentication and registration.
/// </summary>
public class AuthController : ApiControllerBase
{
    /// <summary>
    /// Registers a new family and owner, returning authentication tokens.
    /// This is the primary signup flow for creating a new family (tenant).
    /// </summary>
    /// <param name="command">The registration command.</param>
    /// <returns>The created family and authentication tokens.</returns>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<FamilyCreationResultDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<FamilyCreationResultDto>>> Register([FromBody] RegisterFamilyCommand command)
    {
        var result = await Mediator.Send(command);
        return CreatedResponse($"/api/families/{result.Family.Id}", result);
    }

    /// <summary>
    /// Gets the currently authenticated user's information.
    /// </summary>
    /// <returns>The current user's information.</returns>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetCurrentUser()
    {
        var result = await Mediator.Send(new GetCurrentUserQuery());
        return OkResponse(result);
    }

    /// <summary>
    /// Checks if an email is available for registration.
    /// </summary>
    /// <param name="email">The email to check.</param>
    /// <returns>True if the email is available.</returns>
    [HttpGet("check-email")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<EmailAvailabilityDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<EmailAvailabilityDto>>> CheckEmailAvailability([FromQuery] string email)
    {
        var result = await Mediator.Send(new CheckEmailAvailabilityQuery { Email = email });
        return OkResponse(result);
    }
}
