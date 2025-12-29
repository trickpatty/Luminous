using Fido2NetLib;
using FluentValidation;
using Luminous.Application.Common.Interfaces;
using Luminous.Application.DTOs;
using Luminous.Domain.Entities;
using Luminous.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Luminous.Application.Features.Auth.Commands;

/// <summary>
/// Command to complete passkey authentication.
/// </summary>
public sealed record PasskeyAuthenticateCompleteCommand : IRequest<PasskeyAuthenticateCompleteResultDto>
{
    /// <summary>
    /// The session ID from the authentication start.
    /// </summary>
    public string SessionId { get; init; } = string.Empty;

    /// <summary>
    /// The assertion response from the authenticator.
    /// </summary>
    public AuthenticatorAssertionRawResponse AssertionResponse { get; init; } = null!;

    /// <summary>
    /// The client IP address.
    /// </summary>
    public string? IpAddress { get; init; }

    /// <summary>
    /// The client user agent.
    /// </summary>
    public string? UserAgent { get; init; }
}

/// <summary>
/// Validator for PasskeyAuthenticateCompleteCommand.
/// </summary>
public sealed class PasskeyAuthenticateCompleteCommandValidator : AbstractValidator<PasskeyAuthenticateCompleteCommand>
{
    public PasskeyAuthenticateCompleteCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("Session ID is required.");

        RuleFor(x => x.AssertionResponse)
            .NotNull().WithMessage("Assertion response is required.");
    }
}

/// <summary>
/// Handler for PasskeyAuthenticateCompleteCommand.
/// </summary>
public sealed class PasskeyAuthenticateCompleteCommandHandler : IRequestHandler<PasskeyAuthenticateCompleteCommand, PasskeyAuthenticateCompleteResultDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebAuthnService _webAuthnService;
    private readonly ITokenService _tokenService;
    private readonly ILogger<PasskeyAuthenticateCompleteCommandHandler> _logger;

    public PasskeyAuthenticateCompleteCommandHandler(
        IUnitOfWork unitOfWork,
        IWebAuthnService webAuthnService,
        ITokenService tokenService,
        ILogger<PasskeyAuthenticateCompleteCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _webAuthnService = webAuthnService;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<PasskeyAuthenticateCompleteResultDto> Handle(
        PasskeyAuthenticateCompleteCommand request,
        CancellationToken cancellationToken)
    {
        // Complete the authentication with the WebAuthn service
        var result = await _webAuthnService.CompleteAuthenticationAsync(
            request.SessionId,
            request.AssertionResponse,
            cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning(
                "Passkey authentication failed for session {SessionId}: {Error}",
                request.SessionId,
                result.Error);

            return new PasskeyAuthenticateCompleteResultDto
            {
                Success = false,
                Error = result.Error ?? "Passkey authentication failed."
            };
        }

        // Get the user using cross-partition query (familyId not available during authentication)
        var user = await _unitOfWork.Users.GetByIdCrossPartitionAsync(
            result.UserId!,
            cancellationToken);

        if (user == null)
        {
            _logger.LogError(
                "User not found after passkey authentication: {UserId}",
                result.UserId);

            return new PasskeyAuthenticateCompleteResultDto
            {
                Success = false,
                Error = "User not found."
            };
        }

        // Update the credential's signature counter
        var credential = await _unitOfWork.Credentials.GetByCredentialIdAsync(
            result.CredentialId!,
            cancellationToken);

        if (credential != null)
        {
            credential.UpdateCounter(result.SignatureCounter);
            await _unitOfWork.Credentials.UpdateAsync(credential, cancellationToken);
        }

        // Record login
        user.RecordLogin();
        await _unitOfWork.Users.UpdateAsync(user, cancellationToken);

        // Create refresh token
        var (refreshToken, rawRefreshToken) = RefreshToken.Create(
            user.Id,
            user.FamilyId,
            TimeSpan.FromDays(7),
            request.IpAddress,
            request.UserAgent);

        await _unitOfWork.RefreshTokens.AddAsync(refreshToken, cancellationToken);

        // Generate access token
        var authResult = _tokenService.GenerateToken(user);
        authResult = authResult with { RefreshToken = rawRefreshToken };

        _logger.LogInformation(
            "User {UserId} authenticated via passkey",
            user.Id);

        return new PasskeyAuthenticateCompleteResultDto
        {
            Success = true,
            Auth = authResult
        };
    }
}
