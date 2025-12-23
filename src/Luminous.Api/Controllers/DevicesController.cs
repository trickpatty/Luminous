using Luminous.Application.DTOs;
using Luminous.Application.Features.Devices.Commands;
using Luminous.Application.Features.Devices.Queries;
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
    /// <returns>The linked device with authentication token.</returns>
    [HttpPost("link")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<LinkedDeviceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<LinkedDeviceDto>>> LinkDevice([FromBody] LinkDeviceRequest request)
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

    /// <summary>
    /// Gets a device by ID.
    /// </summary>
    /// <param name="familyId">The family ID.</param>
    /// <param name="id">The device ID.</param>
    /// <returns>The device.</returns>
    [HttpGet("family/{familyId}/{id}")]
    [Authorize(Policy = "FamilyMember")]
    [ProducesResponseType(typeof(ApiResponse<DeviceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<DeviceDto>>> GetDevice(string familyId, string id)
    {
        var query = new GetDeviceQuery
        {
            DeviceId = id,
            FamilyId = familyId
        };
        var result = await Mediator.Send(query);
        return OkResponse(result);
    }

    /// <summary>
    /// Gets all devices for a family.
    /// </summary>
    /// <param name="familyId">The family ID.</param>
    /// <param name="activeOnly">If true, only returns active devices.</param>
    /// <returns>The list of devices.</returns>
    [HttpGet("family/{familyId}")]
    [Authorize(Policy = "FamilyMember")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<DeviceDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DeviceDto>>>> GetFamilyDevices(
        string familyId,
        [FromQuery] bool? activeOnly = null)
    {
        var query = new GetFamilyDevicesQuery
        {
            FamilyId = familyId,
            ActiveOnly = activeOnly
        };
        var result = await Mediator.Send(query);
        return OkResponse(result);
    }

    /// <summary>
    /// Updates a device.
    /// </summary>
    /// <param name="familyId">The family ID.</param>
    /// <param name="id">The device ID.</param>
    /// <param name="request">The update request.</param>
    /// <returns>The updated device.</returns>
    [HttpPut("family/{familyId}/{id}")]
    [Authorize(Policy = "FamilyAdmin")]
    [ProducesResponseType(typeof(ApiResponse<DeviceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<DeviceDto>>> UpdateDevice(
        string familyId,
        string id,
        [FromBody] UpdateDeviceRequest request)
    {
        var command = new UpdateDeviceCommand
        {
            DeviceId = id,
            FamilyId = familyId,
            Name = request.Name,
            Settings = request.Settings
        };
        var result = await Mediator.Send(command);
        return OkResponse(result);
    }

    /// <summary>
    /// Unlinks a device from a family.
    /// </summary>
    /// <param name="familyId">The family ID.</param>
    /// <param name="id">The device ID.</param>
    /// <returns>No content on success.</returns>
    [HttpPost("family/{familyId}/{id}/unlink")]
    [Authorize(Policy = "FamilyAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnlinkDevice(string familyId, string id)
    {
        var command = new UnlinkDeviceCommand
        {
            DeviceId = id,
            FamilyId = familyId
        };
        await Mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Deletes a device.
    /// </summary>
    /// <param name="familyId">The family ID.</param>
    /// <param name="id">The device ID.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("family/{familyId}/{id}")]
    [Authorize(Policy = "FamilyAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDevice(string familyId, string id)
    {
        var command = new DeleteDeviceCommand
        {
            DeviceId = id,
            FamilyId = familyId
        };
        await Mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Records a device heartbeat (updates last seen timestamp).
    /// </summary>
    /// <param name="familyId">The family ID.</param>
    /// <param name="id">The device ID.</param>
    /// <param name="request">The heartbeat request.</param>
    /// <returns>The heartbeat response.</returns>
    [HttpPost("family/{familyId}/{id}/heartbeat")]
    [Authorize(Policy = "FamilyMember")]
    [ProducesResponseType(typeof(ApiResponse<DeviceHeartbeatDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<DeviceHeartbeatDto>>> RecordHeartbeat(
        string familyId,
        string id,
        [FromBody] RecordHeartbeatRequest request)
    {
        var command = new RecordHeartbeatCommand
        {
            DeviceId = id,
            FamilyId = familyId,
            AppVersion = request.AppVersion
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

/// <summary>
/// Request to update a device.
/// </summary>
public record UpdateDeviceRequest
{
    public string? Name { get; init; }
    public DeviceSettingsDto? Settings { get; init; }
}

/// <summary>
/// Request to record a device heartbeat.
/// </summary>
public record RecordHeartbeatRequest
{
    public string? AppVersion { get; init; }
}
