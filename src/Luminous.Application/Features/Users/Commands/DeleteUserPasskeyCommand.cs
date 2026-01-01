using FluentValidation;
using Luminous.Application.Common.Exceptions;
using Luminous.Application.Common.Interfaces;
using Luminous.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Luminous.Application.Features.Users.Commands;

/// <summary>
/// Command to delete a passkey for a specific user (admin only).
/// </summary>
public sealed record DeleteUserPasskeyCommand : IRequest<bool>
{
    /// <summary>
    /// The family ID the user belongs to.
    /// </summary>
    public string FamilyId { get; init; } = string.Empty;

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
/// Validator for DeleteUserPasskeyCommand.
/// </summary>
public sealed class DeleteUserPasskeyCommandValidator : AbstractValidator<DeleteUserPasskeyCommand>
{
    public DeleteUserPasskeyCommandValidator()
    {
        RuleFor(x => x.FamilyId)
            .NotEmpty().WithMessage("Family ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.PasskeyId)
            .NotEmpty().WithMessage("Passkey ID is required.");
    }
}

/// <summary>
/// Handler for DeleteUserPasskeyCommand.
/// </summary>
public sealed class DeleteUserPasskeyCommandHandler : IRequestHandler<DeleteUserPasskeyCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<DeleteUserPasskeyCommandHandler> _logger;

    public DeleteUserPasskeyCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<DeleteUserPasskeyCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<bool> Handle(
        DeleteUserPasskeyCommand request,
        CancellationToken cancellationToken)
    {
        // Validate tenant access
        _tenantContext.EnsureAccessToFamily(request.FamilyId);

        // Verify user exists in the family
        var user = await _unitOfWork.Users.GetByIdAsync(
            request.UserId,
            request.FamilyId,
            cancellationToken);

        if (user == null)
        {
            throw new NotFoundException("User", request.UserId);
        }

        // Get the credential
        var credential = await _unitOfWork.Credentials.GetByIdAsync(
            request.PasskeyId,
            request.UserId,
            cancellationToken);

        if (credential == null)
        {
            throw new NotFoundException($"Passkey with ID '{request.PasskeyId}' not found.");
        }

        // Verify credential belongs to the user
        if (credential.UserId != request.UserId)
        {
            throw new ForbiddenAccessException("Passkey does not belong to the specified user.");
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
                "Admin deactivated passkey {PasskeyId} for user {UserId} in family {FamilyId} (last active passkey)",
                request.PasskeyId,
                request.UserId,
                request.FamilyId);
        }
        else
        {
            // Hard delete
            await _unitOfWork.Credentials.DeleteAsync(credential, cancellationToken);

            _logger.LogInformation(
                "Admin deleted passkey {PasskeyId} for user {UserId} in family {FamilyId}",
                request.PasskeyId,
                request.UserId,
                request.FamilyId);
        }

        return true;
    }
}
