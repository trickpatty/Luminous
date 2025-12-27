using FluentValidation;
using Luminous.Application.Common.Exceptions;
using Luminous.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Luminous.Application.Features.Auth.Commands;

/// <summary>
/// Command to delete a passkey.
/// </summary>
public sealed record DeletePasskeyCommand : IRequest<bool>
{
    /// <summary>
    /// The user ID who owns the passkey.
    /// </summary>
    public string UserId { get; init; } = string.Empty;

    /// <summary>
    /// The passkey ID to delete.
    /// </summary>
    public string PasskeyId { get; init; } = string.Empty;
}

/// <summary>
/// Validator for DeletePasskeyCommand.
/// </summary>
public sealed class DeletePasskeyCommandValidator : AbstractValidator<DeletePasskeyCommand>
{
    public DeletePasskeyCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.PasskeyId)
            .NotEmpty().WithMessage("Passkey ID is required.");
    }
}

/// <summary>
/// Handler for DeletePasskeyCommand.
/// </summary>
public sealed class DeletePasskeyCommandHandler : IRequestHandler<DeletePasskeyCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeletePasskeyCommandHandler> _logger;

    public DeletePasskeyCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<DeletePasskeyCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(
        DeletePasskeyCommand request,
        CancellationToken cancellationToken)
    {
        // Get the credential
        var credential = await _unitOfWork.Credentials.GetByIdAsync(
            request.PasskeyId,
            request.UserId,
            cancellationToken);

        if (credential == null)
        {
            throw new NotFoundException($"Passkey with ID '{request.PasskeyId}' not found.");
        }

        // Verify ownership
        if (credential.UserId != request.UserId)
        {
            throw new ForbiddenAccessException("You do not have permission to delete this passkey.");
        }

        // Check if this is the user's last active passkey
        var userCredentials = await _unitOfWork.Credentials.GetByUserIdAsync(
            request.UserId,
            cancellationToken);

        var activeCredentialsCount = userCredentials.Count(c => c.IsActive);
        if (activeCredentialsCount <= 1 && credential.IsActive)
        {
            // Soft delete - just mark as inactive
            credential.IsActive = false;
            credential.ModifiedAt = DateTime.UtcNow;
            await _unitOfWork.Credentials.UpdateAsync(credential, cancellationToken);

            _logger.LogInformation(
                "Passkey {PasskeyId} deactivated for user {UserId} (last active passkey)",
                request.PasskeyId,
                request.UserId);
        }
        else
        {
            // Hard delete
            await _unitOfWork.Credentials.DeleteAsync(credential, cancellationToken);

            _logger.LogInformation(
                "Passkey {PasskeyId} deleted for user {UserId}",
                request.PasskeyId,
                request.UserId);
        }

        return true;
    }
}
