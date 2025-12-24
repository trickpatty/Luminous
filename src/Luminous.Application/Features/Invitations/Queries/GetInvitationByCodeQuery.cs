using FluentValidation;
using Luminous.Application.Common.Exceptions;
using Luminous.Application.DTOs;
using Luminous.Domain.Interfaces;
using MediatR;

namespace Luminous.Application.Features.Invitations.Queries;

/// <summary>
/// Query to get an invitation by its unique code.
/// This does not require authentication (used during the accept flow).
/// </summary>
public sealed record GetInvitationByCodeQuery : IRequest<InvitationDto>
{
    public string Code { get; init; } = string.Empty;
}

/// <summary>
/// Validator for GetInvitationByCodeQuery.
/// </summary>
public sealed class GetInvitationByCodeQueryValidator : AbstractValidator<GetInvitationByCodeQuery>
{
    public GetInvitationByCodeQueryValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Invitation code is required.");
    }
}

/// <summary>
/// Handler for GetInvitationByCodeQuery.
/// </summary>
public sealed class GetInvitationByCodeQueryHandler : IRequestHandler<GetInvitationByCodeQuery, InvitationDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetInvitationByCodeQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<InvitationDto> Handle(GetInvitationByCodeQuery request, CancellationToken cancellationToken)
    {
        var invitation = await _unitOfWork.Invitations.GetByCodeAsync(request.Code, cancellationToken)
            ?? throw new NotFoundException("Invitation", request.Code);

        // Get the family name for the response
        var family = await _unitOfWork.Families.GetByIdAsync(invitation.FamilyId, cancellationToken);

        return new InvitationDto
        {
            Id = invitation.Id,
            FamilyId = invitation.FamilyId,
            FamilyName = family?.Name ?? "Unknown Family",
            Email = invitation.Email,
            Role = invitation.Role,
            Code = invitation.Code,
            ExpiresAt = invitation.ExpiresAt,
            Status = invitation.Status,
            Message = invitation.Message,
            CreatedAt = invitation.CreatedAt,
            CreatedBy = invitation.CreatedBy,
            IsValid = invitation.IsValid,
            AcceptedAt = invitation.AcceptedAt,
            AcceptedUserId = invitation.AcceptedUserId
        };
    }
}
