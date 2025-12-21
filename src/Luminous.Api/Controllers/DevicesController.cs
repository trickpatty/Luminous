using Luminous.Application.DTOs;
using Luminous.Application.Features.Devices.Commands;
using Luminous.Domain.Enums;
using Luminous.Shared.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Luminous.Api.Controllers;

/// <summary>
/// Controller for device management.
/// </summary>
public class DevicesController : ApiControllerBase
{
    /// <summary>
    /// Generates a link code for a new device.
    /// </summary>
    /// <param name="request">The link code request.</param>
    /// <returns>The generated link code.</returns>
    [HttpPost("link-code")]
    [ProducesResponseType(typeof(ApiResponse<DeviceLinkCodeDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<DeviceLinkCodeDto>>> GenerateLinkCode(
        [FromBody] GenerateLinkCodeRequest request)
    {
        var command = new GenerateLinkCodeCommand
        {
            DeviceType = request.DeviceType,
            Platform = request.Platform
        };
        var result = await Mediator.Send(command);
        return CreatedResponse($"/api/devices/{result.DeviceId}", result);
    }

    /// <summary>
    /// Links a device to a family using a link code.
    /// </summary>
    /// <param name="request">The link request.</param>
    /// <returns>The linked device.</returns>
    [HttpPost("link")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<DeviceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<DeviceDto>>> LinkDevice([FromBody] LinkDeviceRequest request)
    {
        var command = new LinkDeviceCommand
        {
            LinkCode = request.LinkCode,
            FamilyId = request.FamilyId,
            DeviceName = request.DeviceName
        };
        var result = await Mediator.Send(command);
        return OkResponse(result);
    }
}

/// <summary>
/// Request to generate a link code.
/// </summary>
public record GenerateLinkCodeRequest
{
    public DeviceType DeviceType { get; init; } = DeviceType.Display;
    public string? Platform { get; init; }
}

/// <summary>
/// Request to link a device.
/// </summary>
public record LinkDeviceRequest
{
    public string LinkCode { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string DeviceName { get; init; } = string.Empty;
}
