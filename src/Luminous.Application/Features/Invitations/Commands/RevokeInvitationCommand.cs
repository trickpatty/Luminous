using FluentValidation;
using Luminous.Application.Common.Exceptions;
using Luminous.Application.Common.Interfaces;
using Luminous.Domain.Enums;
using Luminous.Domain.Interfaces;
using MediatR;

namespace Luminous.Application.Features.Invitations.Commands;

/// <summary>
/// Command to revoke a pending family invitation.
/// </summary>
public sealed record RevokeInvitationCommand : IRequest<Unit>
{
    public string FamilyId { get; init; } = string.Empty;
    public string InvitationId { get; init; } = string.Empty;
}

/// <summary>
/// Validator for RevokeInvitationCommand.
/// </summary>
public sealed class RevokeInvitationCommandValidator : AbstractValidator<RevokeInvitationCommand>
{
    public RevokeInvitationCommandValidator()
    {
        RuleFor(x => x.FamilyId)
            .NotEmpty().WithMessage("Family ID is required.");

        RuleFor(x => x.InvitationId)
            .NotEmpty().WithMessage("Invitation ID is required.");
    }
}

/// <summary>
/// Handler for RevokeInvitationCommand.
/// </summary>
public sealed class RevokeInvitationCommandHandler : IRequestHandler<RevokeInvitationCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public RevokeInvitationCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(RevokeInvitationCommand request, CancellationToken cancellationToken)
    {
        // Verify the user is authorized (must be Owner or Admin)
        var currentUserRole = Enum.Parse<UserRole>(_currentUserService.Role ?? UserRole.Child.ToString(), true);
        if (currentUserRole != UserRole.Owner && currentUserRole != UserRole.Admin)
        {
            throw new ForbiddenAccessException("Only Owners and Admins can revoke invitations.");
        }

        // Find the invitation
        var invitation = await _unitOfWork.Invitations.GetByIdAsync(request.InvitationId, request.FamilyId, cancellationToken)
            ?? throw new NotFoundException("Invitation", request.InvitationId);

        // Verify the invitation belongs to the family
        if (invitation.FamilyId != request.FamilyId)
        {
            throw new ForbiddenAccessException("You don't have access to this invitation.");
        }

        // Check if invitation can be revoked
        if (invitation.Status != InvitationStatus.Pending)
        {
            var message = invitation.Status switch
            {
                InvitationStatus.Accepted => "Cannot revoke an accepted invitation.",
                InvitationStatus.Declined => "Cannot revoke a declined invitation.",
                InvitationStatus.Revoked => "This invitation has already been revoked.",
                InvitationStatus.Expired => "Cannot revoke an expired invitation.",
                _ => "This invitation cannot be revoked."
            };
            throw new FluentValidation.ValidationException([new FluentValidation.Results.ValidationFailure(
                "InvitationId", message)]);
        }

        // Revoke the invitation
        invitation.Revoke(_currentUserService.UserId ?? "system");
        await _unitOfWork.Invitations.UpdateAsync(invitation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
