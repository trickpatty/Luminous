using Luminous.Api.Configuration;
using Luminous.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Luminous.Api.Controllers;

/// <summary>
/// Development-only authentication controller for local JWT token generation.
/// This controller is only available when EnableLocalTokenGeneration is true.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class DevAuthController : ControllerBase
{
    private readonly ILocalJwtTokenService _jwtService;
    private readonly JwtSettings _jwtSettings;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<DevAuthController> _logger;

    public DevAuthController(
        ILocalJwtTokenService jwtService,
        IOptions<JwtSettings> jwtSettings,
        IWebHostEnvironment environment,
        ILogger<DevAuthController> logger)
    {
        _jwtService = jwtService;
        _jwtSettings = jwtSettings.Value;
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// Generates a development JWT token with default test user credentials.
    /// Only available in development environments.
    /// </summary>
    /// <returns>A JWT access token for development use.</returns>
    [HttpPost("token")]
    [ProducesResponseType(typeof(DevTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult GetDevToken()
    {
        if (!CanGenerateTokens())
        {
            _logger.LogWarning("Attempted to generate dev token in non-development environment");
            return Forbid();
        }

        var token = _jwtService.GenerateDevToken();
        return Ok(new DevTokenResponse
        {
            AccessToken = token,
            TokenType = "Bearer",
            ExpiresIn = _jwtSettings.ExpirationMinutes * 60,
            User = new DevUserInfo
            {
                UserId = "dev-user-001",
                FamilyId = "dev-family-001",
                Email = "dev@luminous.local",
                DisplayName = "Developer",
                Role = "Owner"
            }
        });
    }

    /// <summary>
    /// Generates a custom development JWT token with specified credentials.
    /// Only available in development environments.
    /// </summary>
    /// <param name="request">The token request with user details.</param>
    /// <returns>A JWT access token for development use.</returns>
    [HttpPost("token/custom")]
    [ProducesResponseType(typeof(DevTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult GetCustomDevToken([FromBody] DevTokenRequest request)
    {
        if (!CanGenerateTokens())
        {
            _logger.LogWarning("Attempted to generate dev token in non-development environment");
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var token = _jwtService.GenerateToken(
            request.UserId,
            request.FamilyId,
            request.Email,
            request.Role,
            request.DisplayName);

        return Ok(new DevTokenResponse
        {
            AccessToken = token,
            TokenType = "Bearer",
            ExpiresIn = _jwtSettings.ExpirationMinutes * 60,
            User = new DevUserInfo
            {
                UserId = request.UserId,
                FamilyId = request.FamilyId,
                Email = request.Email,
                DisplayName = request.DisplayName,
                Role = request.Role
            }
        });
    }

    /// <summary>
    /// Checks if the development authentication is available.
    /// </summary>
    [HttpGet("status")]
    [ProducesResponseType(typeof(DevAuthStatus), StatusCodes.Status200OK)]
    public IActionResult GetStatus()
    {
        return Ok(new DevAuthStatus
        {
            IsEnabled = CanGenerateTokens(),
            Environment = _environment.EnvironmentName,
            Message = CanGenerateTokens()
                ? "Development authentication is enabled. Use POST /api/devauth/token to get a token."
                : "Development authentication is disabled. This feature is only available in development environments."
        });
    }

    private bool CanGenerateTokens()
    {
        return _environment.IsDevelopment() && _jwtSettings.EnableLocalTokenGeneration;
    }
}

/// <summary>
/// Request model for custom development token generation.
/// </summary>
public class DevTokenRequest
{
    /// <summary>
    /// The user ID to include in the token.
    /// </summary>
    public required string UserId { get; init; }

    /// <summary>
    /// The family ID to include in the token.
    /// </summary>
    public required string FamilyId { get; init; }

    /// <summary>
    /// The email to include in the token.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// The user's role (Owner, Admin, Adult, Teen, Child, Caregiver).
    /// </summary>
    public required string Role { get; init; }

    /// <summary>
    /// The user's display name.
    /// </summary>
    public required string DisplayName { get; init; }
}

/// <summary>
/// Response model for development token generation.
/// </summary>
public class DevTokenResponse
{
    /// <summary>
    /// The JWT access token.
    /// </summary>
    public required string AccessToken { get; init; }

    /// <summary>
    /// The token type (always "Bearer").
    /// </summary>
    public required string TokenType { get; init; }

    /// <summary>
    /// The token expiration time in seconds.
    /// </summary>
    public required int ExpiresIn { get; init; }

    /// <summary>
    /// Information about the development user.
    /// </summary>
    public required DevUserInfo User { get; init; }
}

/// <summary>
/// Information about the development user.
/// </summary>
public class DevUserInfo
{
    public required string UserId { get; init; }
    public required string FamilyId { get; init; }
    public required string Email { get; init; }
    public required string DisplayName { get; init; }
    public required string Role { get; init; }
}

/// <summary>
/// Status of the development authentication feature.
/// </summary>
public class DevAuthStatus
{
    /// <summary>
    /// Whether development authentication is enabled.
    /// </summary>
    public required bool IsEnabled { get; init; }

    /// <summary>
    /// The current environment name.
    /// </summary>
    public required string Environment { get; init; }

    /// <summary>
    /// A message describing the current status.
    /// </summary>
    public required string Message { get; init; }
}
