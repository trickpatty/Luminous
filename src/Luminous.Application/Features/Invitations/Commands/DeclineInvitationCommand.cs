using FluentValidation;
using Luminous.Application.Common.Exceptions;
using Luminous.Domain.Enums;
using Luminous.Domain.Interfaces;
using MediatR;

namespace Luminous.Application.Features.Invitations.Commands;

/// <summary>
/// Command to decline a family invitation.
/// </summary>
public sealed record DeclineInvitationCommand : IRequest<Unit>
{
    public string Code { get; init; } = string.Empty;
}

/// <summary>
/// Validator for DeclineInvitationCommand.
/// </summary>
public sealed class DeclineInvitationCommandValidator : AbstractValidator<DeclineInvitationCommand>
{
    public DeclineInvitationCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Invitation code is required.");
    }
}

/// <summary>
/// Handler for DeclineInvitationCommand.
/// </summary>
public sealed class DeclineInvitationCommandHandler : IRequestHandler<DeclineInvitationCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeclineInvitationCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeclineInvitationCommand request, CancellationToken cancellationToken)
    {
        // Find the invitation by code
        var invitation = await _unitOfWork.Invitations.GetByCodeAsync(request.Code, cancellationToken)
            ?? throw new NotFoundException("Invitation", request.Code);

        // Check if invitation can be declined
        if (invitation.Status != InvitationStatus.Pending)
        {
            var message = invitation.Status switch
            {
                InvitationStatus.Accepted => "This invitation has already been accepted.",
                InvitationStatus.Declined => "This invitation has already been declined.",
                InvitationStatus.Revoked => "This invitation has been revoked.",
                InvitationStatus.Expired => "This invitation has expired.",
                _ => "This invitation cannot be declined."
            };
            throw new FluentValidation.ValidationException([new FluentValidation.Results.ValidationFailure(
                "Code", message)]);
        }

        // Decline the invitation
        invitation.Decline();
        await _unitOfWork.Invitations.UpdateAsync(invitation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
