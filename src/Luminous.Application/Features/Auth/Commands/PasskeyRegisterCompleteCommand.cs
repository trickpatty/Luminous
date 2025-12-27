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
/// Command to complete passkey registration.
/// </summary>
public sealed record PasskeyRegisterCompleteCommand : IRequest<PasskeyRegisterCompleteResultDto>
{
    /// <summary>
    /// The session ID from the registration start.
    /// </summary>
    public string SessionId { get; init; } = string.Empty;

    /// <summary>
    /// The attestation response from the authenticator.
    /// </summary>
    public AuthenticatorAttestationRawResponse AttestationResponse { get; init; } = null!;

    /// <summary>
    /// Optional display name for the passkey.
    /// </summary>
    public string? DisplayName { get; init; }
}

/// <summary>
/// Validator for PasskeyRegisterCompleteCommand.
/// </summary>
public sealed class PasskeyRegisterCompleteCommandValidator : AbstractValidator<PasskeyRegisterCompleteCommand>
{
    public PasskeyRegisterCompleteCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("Session ID is required.");

        RuleFor(x => x.AttestationResponse)
            .NotNull().WithMessage("Attestation response is required.");

        RuleFor(x => x.DisplayName)
            .MaximumLength(64).When(x => x.DisplayName != null)
            .WithMessage("Display name must not exceed 64 characters.");
    }
}

/// <summary>
/// Handler for PasskeyRegisterCompleteCommand.
/// </summary>
public sealed class PasskeyRegisterCompleteCommandHandler : IRequestHandler<PasskeyRegisterCompleteCommand, PasskeyRegisterCompleteResultDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebAuthnService _webAuthnService;
    private readonly ILogger<PasskeyRegisterCompleteCommandHandler> _logger;

    public PasskeyRegisterCompleteCommandHandler(
        IUnitOfWork unitOfWork,
        IWebAuthnService webAuthnService,
        ILogger<PasskeyRegisterCompleteCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _webAuthnService = webAuthnService;
        _logger = logger;
    }

    public async Task<PasskeyRegisterCompleteResultDto> Handle(
        PasskeyRegisterCompleteCommand request,
        CancellationToken cancellationToken)
    {
        // Complete the registration with the WebAuthn service
        var result = await _webAuthnService.CompleteRegistrationAsync(
            request.SessionId,
            request.AttestationResponse,
            cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning(
                "Passkey registration failed for session {SessionId}: {Error}",
                request.SessionId,
                result.Error);

            return new PasskeyRegisterCompleteResultDto
            {
                Success = false,
                Error = result.Error ?? "Passkey registration failed."
            };
        }

        // Create and store the credential
        var credential = Credential.Create(
            result.UserId!,
            result.CredentialId!,
            result.PublicKey!,
            result.UserHandle!,
            result.SignatureCounter,
            result.AaGuid,
            request.DisplayName ?? "Passkey",
            isDiscoverable: true);

        await _unitOfWork.Credentials.AddAsync(credential, cancellationToken);

        _logger.LogInformation(
            "Passkey registered successfully for user {UserId}, credential {CredentialId}",
            result.UserId,
            Convert.ToBase64String(result.CredentialId!));

        return new PasskeyRegisterCompleteResultDto
        {
            Success = true,
            CredentialId = Convert.ToBase64String(result.CredentialId!)
        };
    }
}
