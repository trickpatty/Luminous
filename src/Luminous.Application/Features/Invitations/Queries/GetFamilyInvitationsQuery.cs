using FluentValidation;
using Luminous.Application.Common.Exceptions;
using Luminous.Application.Common.Interfaces;
using Luminous.Application.DTOs;
using Luminous.Domain.Enums;
using Luminous.Domain.Interfaces;
using MediatR;

namespace Luminous.Application.Features.Invitations.Queries;

/// <summary>
/// Query to get all invitations for a family.
/// </summary>
public sealed record GetFamilyInvitationsQuery : IRequest<IReadOnlyList<InvitationDto>>
{
    public string FamilyId { get; init; } = string.Empty;
    public InvitationStatus? Status { get; init; }
}

/// <summary>
/// Validator for GetFamilyInvitationsQuery.
/// </summary>
public sealed class GetFamilyInvitationsQueryValidator : AbstractValidator<GetFamilyInvitationsQuery>
{
    public GetFamilyInvitationsQueryValidator()
    {
        RuleFor(x => x.FamilyId)
            .NotEmpty().WithMessage("Family ID is required.");
    }
}

/// <summary>
/// Handler for GetFamilyInvitationsQuery.
/// </summary>
public sealed class GetFamilyInvitationsQueryHandler : IRequestHandler<GetFamilyInvitationsQuery, IReadOnlyList<InvitationDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public GetFamilyInvitationsQueryHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<InvitationDto>> Handle(GetFamilyInvitationsQuery request, CancellationToken cancellationToken)
    {
        // Validate tenant access
        _tenantContext.EnsureAccessToFamily(request.FamilyId);

        // Verify the user is authorized (must be Owner or Admin)
        var currentUserRole = Enum.Parse<UserRole>(_currentUserService.Role ?? UserRole.Child.ToString(), true);
        if (currentUserRole != UserRole.Owner && currentUserRole != UserRole.Admin)
        {
            throw new ForbiddenAccessException("Only Owners and Admins can view invitations.");
        }

        // Get the family for the response
        var family = await _unitOfWork.Families.GetByIdAsync(request.FamilyId, cancellationToken)
            ?? throw new NotFoundException("Family", request.FamilyId);

        // Get invitations
        var invitations = request.Status.HasValue
            ? await _unitOfWork.Invitations.GetByFamilyIdAndStatusAsync(request.FamilyId, request.Status.Value, cancellationToken)
            : await _unitOfWork.Invitations.GetByFamilyIdAsync(request.FamilyId, cancellationToken);

        return invitations.Select(invitation => new InvitationDto
        {
            Id = invitation.Id,
            FamilyId = invitation.FamilyId,
            FamilyName = family.Name,
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
        }).ToList();
    }
}
