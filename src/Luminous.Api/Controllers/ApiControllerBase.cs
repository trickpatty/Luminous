using Luminous.Shared.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Luminous.Api.Controllers;

/// <summary>
/// Base controller with common functionality.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? _mediator;

    /// <summary>
    /// Gets the MediatR sender.
    /// </summary>
    protected ISender Mediator => _mediator ??=
        HttpContext.RequestServices.GetRequiredService<ISender>();

    /// <summary>
    /// Creates an OK response with data.
    /// </summary>
    protected ActionResult<ApiResponse<T>> OkResponse<T>(T data)
    {
        return Ok(ApiResponse<T>.Ok(data));
    }

    /// <summary>
    /// Creates a Created response with data.
    /// </summary>
    protected ActionResult<ApiResponse<T>> CreatedResponse<T>(string location, T data)
    {
        return Created(location, ApiResponse<T>.Ok(data));
    }

    /// <summary>
    /// Creates a No Content response.
    /// </summary>
    protected new ActionResult NoContent()
    {
        return base.NoContent();
    }
}
