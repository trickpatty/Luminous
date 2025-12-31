using FluentValidation;
using Luminous.Application.Common.Interfaces;
using Luminous.Application.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Luminous.Application.Features.Auth.Commands;

/// <summary>
/// Command to start passkey authentication.
/// </summary>
public sealed record PasskeyAuthenticateStartCommand : IRequest<PasskeyAuthenticateStartResultDto>
{
    /// <summary>
    /// Optional email address for non-discoverable credentials.
    /// If not provided, discoverable credentials will be used.
    /// </summary>
    public string? Email { get; init; }
}

/// <summary>
/// Validator for PasskeyAuthenticateStartCommand.
/// </summary>
public sealed class PasskeyAuthenticateStartCommandValidator : AbstractValidator<PasskeyAuthenticateStartCommand>
{
    public PasskeyAuthenticateStartCommandValidator()
    {
        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("A valid email address is required.");
    }
}

/// <summary>
/// Handler for PasskeyAuthenticateStartCommand.
/// </summary>
public sealed class PasskeyAuthenticateStartCommandHandler : IRequestHandler<PasskeyAuthenticateStartCommand, PasskeyAuthenticateStartResultDto>
{
    private readonly IWebAuthnService _webAuthnService;
    private readonly ILogger<PasskeyAuthenticateStartCommandHandler> _logger;

    public PasskeyAuthenticateStartCommandHandler(
        IWebAuthnService webAuthnService,
        ILogger<PasskeyAuthenticateStartCommandHandler> logger)
    {
        _webAuthnService = webAuthnService;
        _logger = logger;
    }

    public async Task<PasskeyAuthenticateStartResultDto> Handle(
        PasskeyAuthenticateStartCommand request,
        CancellationToken cancellationToken)
    {
        // Create authentication options
        var (options, sessionId) = await _webAuthnService.CreateAuthenticationOptionsAsync(
            request.Email,
            cancellationToken);

        _logger.LogInformation(
            "Started passkey authentication{Email}, session {SessionId}",
            string.IsNullOrEmpty(request.Email) ? "" : $" for {request.Email}",
            sessionId);

        return new PasskeyAuthenticateStartResultDto
        {
            // Use Fido2NetLib's ToJson() for WebAuthn-compliant serialization
            Options = options.ToJson(),
            SessionId = sessionId
        };
    }
}
