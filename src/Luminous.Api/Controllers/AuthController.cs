using Fido2NetLib;
using Luminous.Application.DTOs;
using Luminous.Application.Features.Auth.Commands;
using Luminous.Application.Features.Auth.Queries;
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
    #region Registration (2-Step with Email Verification)

    /// <summary>
    /// Starts the registration process by sending an OTP to verify email ownership.
    /// Step 1 of 2: Validates email, sends verification code, and returns a session ID.
    /// </summary>
    /// <param name="request">The registration request details.</param>
    /// <returns>Session ID and email verification status.</returns>
    [HttpPost("register/start")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<RegisterStartResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ApiResponse<RegisterStartResultDto>>> StartRegistration(
        [FromBody] RegisterStartRequest request)
    {
        var command = new RegisterStartCommand
        {
            Email = request.Email,
            DisplayName = request.DisplayName,
            FamilyName = request.FamilyName,
            Timezone = request.Timezone ?? "UTC",
            InviteCode = request.InviteCode,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers.UserAgent.ToString()
        };

        var result = await Mediator.Send(command);

        if (!result.Success)
        {
            return BadRequest(ApiResponse<RegisterStartResultDto>.Fail("RATE_LIMITED", result.Message));
        }

        return OkResponse(result);
    }

    /// <summary>
    /// Completes the registration process by verifying the OTP and creating the account.
    /// Step 2 of 2: Verifies email ownership, creates family and user, returns auth tokens.
    /// </summary>
    /// <param name="request">The verification request with session ID and OTP code.</param>
    /// <returns>The created family and authentication tokens.</returns>
    [HttpPost("register/complete")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<RegisterCompleteResultDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<RegisterCompleteResultDto>>> CompleteRegistration(
        [FromBody] RegisterCompleteRequest request)
    {
        var command = new RegisterCompleteCommand
        {
            SessionId = request.SessionId,
            Code = request.Code,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers.UserAgent.ToString()
        };

        var result = await Mediator.Send(command);

        if (!result.Success)
        {
            if (result.RemainingAttempts > 0)
            {
                return BadRequest(ApiResponse<RegisterCompleteResultDto>.Fail("INVALID_CODE", result.Error ?? "Invalid verification code"));
            }
            return Unauthorized(ApiResponse<RegisterCompleteResultDto>.Fail("VERIFICATION_FAILED", result.Error ?? "Verification failed"));
        }

        return Created($"/api/families/{result.Family!.Id}", ApiResponse<RegisterCompleteResultDto>.Ok(result));
    }

    #endregion

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

    #region OTP Authentication

    /// <summary>
    /// Requests an OTP for email-based authentication.
    /// </summary>
    /// <param name="request">The OTP request details.</param>
    /// <returns>Information about the OTP request.</returns>
    [HttpPost("otp/request")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<OtpRequestResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ApiResponse<OtpRequestResultDto>>> RequestOtp([FromBody] OtpRequest request)
    {
        var command = new RequestOtpCommand
        {
            Email = request.Email,
            Purpose = request.Purpose ?? "login",
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers.UserAgent.ToString()
        };

        var result = await Mediator.Send(command);
        return OkResponse(result);
    }

    /// <summary>
    /// Verifies an OTP and authenticates the user.
    /// </summary>
    /// <param name="request">The OTP verification details.</param>
    /// <returns>Authentication tokens if successful.</returns>
    [HttpPost("otp/verify")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<OtpVerifyResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<OtpVerifyResultDto>>> VerifyOtp([FromBody] OtpVerifyRequest request)
    {
        var command = new VerifyOtpCommand
        {
            Email = request.Email,
            Code = request.Code,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers.UserAgent.ToString()
        };

        var result = await Mediator.Send(command);

        if (!result.Success)
        {
            return Unauthorized(ApiResponse<OtpVerifyResultDto>.Fail("INVALID_OTP", result.Error ?? "Invalid OTP"));
        }

        return OkResponse(result);
    }

    #endregion

    #region Passkey Registration

    /// <summary>
    /// Starts passkey registration for the authenticated user.
    /// </summary>
    /// <param name="request">Optional display name for the passkey.</param>
    /// <returns>WebAuthn credential creation options.</returns>
    [HttpPost("passkey/register/start")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PasskeyRegisterStartResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<PasskeyRegisterStartResultDto>>> StartPasskeyRegistration(
        [FromBody] PasskeyRegisterRequest? request = null)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<PasskeyRegisterStartResultDto>.Fail("UNAUTHORIZED", "User not authenticated"));
        }

        var command = new PasskeyRegisterStartCommand
        {
            UserId = userId,
            DisplayName = request?.DisplayName
        };

        var result = await Mediator.Send(command);
        return OkResponse(result);
    }

    /// <summary>
    /// Completes passkey registration.
    /// </summary>
    /// <param name="request">The attestation response from the authenticator.</param>
    /// <returns>Registration result.</returns>
    [HttpPost("passkey/register/complete")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PasskeyRegisterCompleteResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<PasskeyRegisterCompleteResultDto>>> CompletePasskeyRegistration(
        [FromBody] PasskeyRegisterCompleteRequest request)
    {
        // Validate required fields
        if (request.AttestationResponse == null)
        {
            return BadRequest(ApiResponse<PasskeyRegisterCompleteResultDto>.Fail("INVALID_REQUEST", "Attestation response is required"));
        }

        if (string.IsNullOrEmpty(request.AttestationResponse.Id) ||
            string.IsNullOrEmpty(request.AttestationResponse.RawId) ||
            request.AttestationResponse.Response == null ||
            string.IsNullOrEmpty(request.AttestationResponse.Response.ClientDataJSON) ||
            string.IsNullOrEmpty(request.AttestationResponse.Response.AttestationObject))
        {
            return BadRequest(ApiResponse<PasskeyRegisterCompleteResultDto>.Fail("INVALID_REQUEST", "Attestation response is incomplete"));
        }

        try
        {
            var command = new PasskeyRegisterCompleteCommand
            {
                SessionId = request.SessionId,
                AttestationResponse = request.ToFido2Response(),
                DisplayName = request.DisplayName
            };

            var result = await Mediator.Send(command);

            if (!result.Success)
            {
                return BadRequest(ApiResponse<PasskeyRegisterCompleteResultDto>.Fail("REGISTRATION_FAILED", result.Error ?? "Registration failed"));
            }

            return OkResponse(result);
        }
        catch (FormatException ex)
        {
            return BadRequest(ApiResponse<PasskeyRegisterCompleteResultDto>.Fail("INVALID_FORMAT", $"Invalid base64url encoding: {ex.Message}"));
        }
    }

    #endregion

    #region Passkey Authentication

    /// <summary>
    /// Starts passkey authentication.
    /// </summary>
    /// <param name="request">Optional email for non-discoverable credentials.</param>
    /// <returns>WebAuthn assertion options.</returns>
    [HttpPost("passkey/authenticate/start")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<PasskeyAuthenticateStartResultDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PasskeyAuthenticateStartResultDto>>> StartPasskeyAuthentication(
        [FromBody] PasskeyAuthenticateStartRequest? request = null)
    {
        var command = new PasskeyAuthenticateStartCommand
        {
            Email = request?.Email
        };

        var result = await Mediator.Send(command);
        return OkResponse(result);
    }

    /// <summary>
    /// Completes passkey authentication.
    /// </summary>
    /// <param name="request">The assertion response from the authenticator.</param>
    /// <returns>Authentication tokens if successful.</returns>
    [HttpPost("passkey/authenticate/complete")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<PasskeyAuthenticateCompleteResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<PasskeyAuthenticateCompleteResultDto>>> CompletePasskeyAuthentication(
        [FromBody] PasskeyAuthenticateCompleteRequest request)
    {
        // Validate required fields
        if (request.AssertionResponse == null)
        {
            return BadRequest(ApiResponse<PasskeyAuthenticateCompleteResultDto>.Fail("INVALID_REQUEST", "Assertion response is required"));
        }

        if (string.IsNullOrEmpty(request.AssertionResponse.Id) ||
            string.IsNullOrEmpty(request.AssertionResponse.RawId) ||
            request.AssertionResponse.Response == null ||
            string.IsNullOrEmpty(request.AssertionResponse.Response.ClientDataJSON) ||
            string.IsNullOrEmpty(request.AssertionResponse.Response.AuthenticatorData) ||
            string.IsNullOrEmpty(request.AssertionResponse.Response.Signature))
        {
            return BadRequest(ApiResponse<PasskeyAuthenticateCompleteResultDto>.Fail("INVALID_REQUEST", "Assertion response is incomplete"));
        }

        try
        {
            var command = new PasskeyAuthenticateCompleteCommand
            {
                SessionId = request.SessionId,
                AssertionResponse = request.ToFido2Response(),
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Request.Headers.UserAgent.ToString()
            };

            var result = await Mediator.Send(command);

            if (!result.Success)
            {
                return Unauthorized(ApiResponse<PasskeyAuthenticateCompleteResultDto>.Fail("AUTHENTICATION_FAILED", result.Error ?? "Authentication failed"));
            }

            return OkResponse(result);
        }
        catch (FormatException ex)
        {
            return BadRequest(ApiResponse<PasskeyAuthenticateCompleteResultDto>.Fail("INVALID_FORMAT", $"Invalid base64url encoding: {ex.Message}"));
        }
    }

    #endregion

    #region Passkey Management

    /// <summary>
    /// Lists the authenticated user's passkeys.
    /// </summary>
    /// <returns>List of passkeys.</returns>
    [HttpGet("passkey/list")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PasskeyListResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<PasskeyListResultDto>>> ListPasskeys()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<PasskeyListResultDto>.Fail("UNAUTHORIZED", "User not authenticated"));
        }

        var result = await Mediator.Send(new ListPasskeysQuery { UserId = userId });
        return OkResponse(result);
    }

    /// <summary>
    /// Deletes a passkey.
    /// </summary>
    /// <param name="id">The passkey ID to delete.</param>
    /// <returns>Success status.</returns>
    [HttpDelete("passkey/{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePasskey([FromRoute] string id)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<bool>.Fail("UNAUTHORIZED", "User not authenticated"));
        }

        await Mediator.Send(new DeletePasskeyCommand
        {
            UserId = userId,
            PasskeyId = id
        });

        return NoContent();
    }

    #endregion

    #region Token Refresh

    /// <summary>
    /// Refreshes an access token using a refresh token.
    /// </summary>
    /// <param name="request">The refresh token.</param>
    /// <returns>New authentication tokens if successful.</returns>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthResultDto>>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var command = new RefreshTokenCommand
        {
            RefreshToken = request.RefreshToken,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers.UserAgent.ToString()
        };

        var result = await Mediator.Send(command);

        if (result == null)
        {
            return Unauthorized(ApiResponse<AuthResultDto>.Fail("INVALID_TOKEN", "Invalid or expired refresh token"));
        }

        return OkResponse(result);
    }

    #endregion
}

#region Request DTOs

/// <summary>
/// Request to start registration (step 1).
/// </summary>
public sealed record RegisterStartRequest
{
    /// <summary>
    /// The email address of the user.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// The display name of the user.
    /// </summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>
    /// The name of the family to create (required if not using invite code).
    /// </summary>
    public string FamilyName { get; init; } = string.Empty;

    /// <summary>
    /// The timezone for the family (IANA timezone ID, required if not using invite code).
    /// </summary>
    public string? Timezone { get; init; }

    /// <summary>
    /// Optional invite code to join an existing family instead of creating a new one.
    /// </summary>
    public string? InviteCode { get; init; }
}

/// <summary>
/// Request to complete registration (step 2).
/// </summary>
public sealed record RegisterCompleteRequest
{
    /// <summary>
    /// The session ID from the registration start.
    /// </summary>
    public string SessionId { get; init; } = string.Empty;

    /// <summary>
    /// The 6-digit verification code.
    /// </summary>
    public string Code { get; init; } = string.Empty;
}

/// <summary>
/// Request to send an OTP.
/// </summary>
public sealed record OtpRequest
{
    /// <summary>
    /// The email address to send the OTP to.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// The purpose of the OTP (login, registration).
    /// </summary>
    public string? Purpose { get; init; }
}

/// <summary>
/// Request to verify an OTP.
/// </summary>
public sealed record OtpVerifyRequest
{
    /// <summary>
    /// The email address the OTP was sent to.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// The 6-digit OTP code.
    /// </summary>
    public string Code { get; init; } = string.Empty;
}

/// <summary>
/// Request to register a passkey.
/// </summary>
public sealed record PasskeyRegisterRequest
{
    /// <summary>
    /// Optional display name for the passkey.
    /// </summary>
    public string? DisplayName { get; init; }
}

/// <summary>
/// Request to complete passkey registration.
/// Uses custom DTO to properly deserialize WebAuthn response from client.
/// </summary>
public sealed record PasskeyRegisterCompleteRequest
{
    /// <summary>
    /// The session ID from the registration start.
    /// </summary>
    public string SessionId { get; init; } = string.Empty;

    /// <summary>
    /// The attestation response from the authenticator.
    /// </summary>
    public AttestationResponseDto AttestationResponse { get; init; } = null!;

    /// <summary>
    /// Optional display name for the passkey.
    /// </summary>
    public string? DisplayName { get; init; }

    /// <summary>
    /// Converts the DTO to Fido2NetLib's AuthenticatorAttestationRawResponse.
    /// </summary>
    public AuthenticatorAttestationRawResponse ToFido2Response()
    {
        return new AuthenticatorAttestationRawResponse
        {
            Id = Base64UrlDecode(AttestationResponse.Id),
            RawId = Base64UrlDecode(AttestationResponse.RawId),
            Type = Fido2NetLib.Objects.PublicKeyCredentialType.PublicKey,
            Response = new AuthenticatorAttestationRawResponse.ResponseData
            {
                ClientDataJson = Base64UrlDecode(AttestationResponse.Response.ClientDataJSON),
                AttestationObject = Base64UrlDecode(AttestationResponse.Response.AttestationObject),
                Transports = AttestationResponse.Response.Transports?.Select(t => Enum.TryParse<Fido2NetLib.Objects.AuthenticatorTransport>(t, true, out var result) ? result : Fido2NetLib.Objects.AuthenticatorTransport.Internal).ToArray()
            }
        };
    }

    private static byte[] Base64UrlDecode(string input)
    {
        // Add padding if needed
        var base64 = input.Replace('-', '+').Replace('_', '/');
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}

/// <summary>
/// DTO for WebAuthn attestation response from client.
/// </summary>
public sealed record AttestationResponseDto
{
    /// <summary>
    /// Base64URL encoded credential ID.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Base64URL encoded raw credential ID.
    /// </summary>
    public string RawId { get; init; } = string.Empty;

    /// <summary>
    /// Credential type (always "public-key").
    /// </summary>
    public string Type { get; init; } = "public-key";

    /// <summary>
    /// The attestation response data.
    /// </summary>
    public AttestationResponseDataDto Response { get; init; } = null!;
}

/// <summary>
/// DTO for WebAuthn attestation response data.
/// </summary>
public sealed record AttestationResponseDataDto
{
    /// <summary>
    /// Base64URL encoded client data JSON.
    /// </summary>
    public string ClientDataJSON { get; init; } = string.Empty;

    /// <summary>
    /// Base64URL encoded attestation object.
    /// </summary>
    public string AttestationObject { get; init; } = string.Empty;

    /// <summary>
    /// Optional list of transports supported by the authenticator.
    /// </summary>
    public string[]? Transports { get; init; }
}

/// <summary>
/// Request to start passkey authentication.
/// </summary>
public sealed record PasskeyAuthenticateStartRequest
{
    /// <summary>
    /// Optional email for non-discoverable credentials.
    /// </summary>
    public string? Email { get; init; }
}

/// <summary>
/// Request to complete passkey authentication.
/// Uses custom DTO to properly deserialize WebAuthn response from client.
/// </summary>
public sealed record PasskeyAuthenticateCompleteRequest
{
    /// <summary>
    /// The session ID from the authentication start.
    /// </summary>
    public string SessionId { get; init; } = string.Empty;

    /// <summary>
    /// The assertion response from the authenticator.
    /// </summary>
    public AssertionResponseDto AssertionResponse { get; init; } = null!;

    /// <summary>
    /// Converts the DTO to Fido2NetLib's AuthenticatorAssertionRawResponse.
    /// </summary>
    public AuthenticatorAssertionRawResponse ToFido2Response()
    {
        return new AuthenticatorAssertionRawResponse
        {
            Id = Base64UrlDecode(AssertionResponse.Id),
            RawId = Base64UrlDecode(AssertionResponse.RawId),
            Type = Fido2NetLib.Objects.PublicKeyCredentialType.PublicKey,
            Response = new AuthenticatorAssertionRawResponse.AssertionResponse
            {
                ClientDataJson = Base64UrlDecode(AssertionResponse.Response.ClientDataJSON),
                AuthenticatorData = Base64UrlDecode(AssertionResponse.Response.AuthenticatorData),
                Signature = Base64UrlDecode(AssertionResponse.Response.Signature),
                UserHandle = string.IsNullOrEmpty(AssertionResponse.Response.UserHandle)
                    ? null
                    : Base64UrlDecode(AssertionResponse.Response.UserHandle)
            }
        };
    }

    private static byte[] Base64UrlDecode(string input)
    {
        // Add padding if needed
        var base64 = input.Replace('-', '+').Replace('_', '/');
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}

/// <summary>
/// DTO for WebAuthn assertion response from client.
/// </summary>
public sealed record AssertionResponseDto
{
    /// <summary>
    /// Base64URL encoded credential ID.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Base64URL encoded raw credential ID.
    /// </summary>
    public string RawId { get; init; } = string.Empty;

    /// <summary>
    /// Credential type (always "public-key").
    /// </summary>
    public string Type { get; init; } = "public-key";

    /// <summary>
    /// The assertion response data.
    /// </summary>
    public AssertionResponseDataDto Response { get; init; } = null!;
}

/// <summary>
/// DTO for WebAuthn assertion response data.
/// </summary>
public sealed record AssertionResponseDataDto
{
    /// <summary>
    /// Base64URL encoded client data JSON.
    /// </summary>
    public string ClientDataJSON { get; init; } = string.Empty;

    /// <summary>
    /// Base64URL encoded authenticator data.
    /// </summary>
    public string AuthenticatorData { get; init; } = string.Empty;

    /// <summary>
    /// Base64URL encoded signature.
    /// </summary>
    public string Signature { get; init; } = string.Empty;

    /// <summary>
    /// Base64URL encoded user handle (optional).
    /// </summary>
    public string? UserHandle { get; init; }
}

/// <summary>
/// Request to refresh a token.
/// </summary>
public sealed record RefreshTokenRequest
{
    /// <summary>
    /// The refresh token.
    /// </summary>
    public string RefreshToken { get; init; } = string.Empty;
}

#endregion
