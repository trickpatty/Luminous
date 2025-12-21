using Luminous.Application.DTOs;
using Luminous.Application.Features.Families.Commands;
using Luminous.Application.Features.Families.Queries;
using Luminous.Shared.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Luminous.Api.Controllers;

/// <summary>
/// Controller for family management.
/// </summary>
public class FamiliesController : ApiControllerBase
{
    /// <summary>
    /// Creates a new family.
    /// </summary>
    /// <param name="command">The create family command.</param>
    /// <returns>The created family.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<FamilyDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<FamilyDto>>> Create([FromBody] CreateFamilyCommand command)
    {
        var result = await Mediator.Send(command);
        return CreatedResponse($"/api/families/{result.Id}", result);
    }

    /// <summary>
    /// Gets the current user's family.
    /// </summary>
    /// <param name="id">The family ID.</param>
    /// <returns>The family details.</returns>
    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<FamilyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<FamilyDto>>> Get(string id)
    {
        var result = await Mediator.Send(new GetFamilyQuery { FamilyId = id });
        return OkResponse(result);
    }

    /// <summary>
    /// Updates family settings.
    /// </summary>
    /// <param name="id">The family ID.</param>
    /// <param name="settings">The updated settings.</param>
    /// <returns>The updated settings.</returns>
    [HttpPut("{id}/settings")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<FamilySettingsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<FamilySettingsDto>>> UpdateSettings(
        string id,
        [FromBody] FamilySettingsDto settings)
    {
        var command = new UpdateFamilySettingsCommand
        {
            FamilyId = id,
            Settings = settings
        };
        var result = await Mediator.Send(command);
        return OkResponse(result);
    }
}
