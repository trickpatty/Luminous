using System.Text.Json;
using FluentValidation;
using Luminous.Application.Common.Interfaces;
using Luminous.Application.DTOs;
using Luminous.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Luminous.Application.Features.Auth.Commands;

/// <summary>
/// Command to start passkey registration for a user.
/// </summary>
public sealed record PasskeyRegisterStartCommand : IRequest<PasskeyRegisterStartResultDto>
{
    /// <summary>
    /// The user ID to register a passkey for.
    /// </summary>
    public string UserId { get; init; } = string.Empty;

    /// <summary>
    /// Optional display name for the passkey.
    /// </summary>
    public string? DisplayName { get; init; }
}

/// <summary>
/// Validator for PasskeyRegisterStartCommand.
/// </summary>
public sealed class PasskeyRegisterStartCommandValidator : AbstractValidator<PasskeyRegisterStartCommand>
{
    public PasskeyRegisterStartCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.DisplayName)
            .MaximumLength(64).When(x => x.DisplayName != null)
            .WithMessage("Display name must not exceed 64 characters.");
    }
}

/// <summary>
/// Handler for PasskeyRegisterStartCommand.
/// </summary>
public sealed class PasskeyRegisterStartCommandHandler : IRequestHandler<PasskeyRegisterStartCommand, PasskeyRegisterStartResultDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebAuthnService _webAuthnService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<PasskeyRegisterStartCommandHandler> _logger;

    public PasskeyRegisterStartCommandHandler(
        IUnitOfWork unitOfWork,
        IWebAuthnService webAuthnService,
        ICurrentUserService currentUserService,
        ILogger<PasskeyRegisterStartCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _webAuthnService = webAuthnService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<PasskeyRegisterStartResultDto> Handle(
        PasskeyRegisterStartCommand request,
        CancellationToken cancellationToken)
    {
        // Get familyId from current user context (required for CosmosDB partition key)
        var familyId = _currentUserService.FamilyId
            ?? throw new UnauthorizedAccessException("Family ID not found in claims.");

        // Get the user
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, familyId, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("User not found for passkey registration: {UserId}", request.UserId);
            throw new KeyNotFoundException($"User with ID '{request.UserId}' not found.");
        }

        // Get existing credentials to exclude
        var existingCredentials = await _unitOfWork.Credentials.GetByUserIdAsync(
            request.UserId,
            cancellationToken);

        var excludeCredentialIds = existingCredentials
            .Where(c => c.IsActive)
            .Select(c => c.CredentialId)
            .ToList();

        // Create registration options
        var (options, sessionId) = await _webAuthnService.CreateRegistrationOptionsAsync(
            request.UserId,
            user.Email,
            user.DisplayName,
            excludeCredentialIds,
            cancellationToken);

        _logger.LogInformation(
            "Started passkey registration for user {UserId}, session {SessionId}",
            request.UserId,
            sessionId);

        return new PasskeyRegisterStartResultDto
        {
            Options = JsonSerializer.Serialize(options),
            SessionId = sessionId
        };
    }
}
