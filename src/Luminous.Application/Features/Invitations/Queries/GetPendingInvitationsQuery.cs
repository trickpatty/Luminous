using FluentValidation;
using Luminous.Application.Common.Exceptions;
using Luminous.Application.Common.Interfaces;
using Luminous.Application.DTOs;
using Luminous.Domain.Enums;
using Luminous.Domain.Interfaces;
using MediatR;

namespace Luminous.Application.Features.Invitations.Queries;

/// <summary>
/// Query to get all pending invitations for a family.
/// </summary>
public sealed record GetPendingInvitationsQuery : IRequest<IReadOnlyList<InvitationDto>>
{
    public string FamilyId { get; init; } = string.Empty;
}

/// <summary>
/// Validator for GetPendingInvitationsQuery.
/// </summary>
public sealed class GetPendingInvitationsQueryValidator : AbstractValidator<GetPendingInvitationsQuery>
{
    public GetPendingInvitationsQueryValidator()
    {
        RuleFor(x => x.FamilyId)
            .NotEmpty().WithMessage("Family ID is required.");
    }
}

/// <summary>
/// Handler for GetPendingInvitationsQuery.
/// </summary>
public sealed class GetPendingInvitationsQueryHandler : IRequestHandler<GetPendingInvitationsQuery, IReadOnlyList<InvitationDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public GetPendingInvitationsQueryHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<InvitationDto>> Handle(GetPendingInvitationsQuery request, CancellationToken cancellationToken)
    {
        // Validate tenant access
        _tenantContext.EnsureAccessToFamily(request.FamilyId);

        // Verify the user is authorized (must be Owner or Admin)
        var currentUserRole = Enum.Parse<UserRole>(_currentUserService.Role ?? UserRole.Child.ToString(), true);
        if (currentUserRole != UserRole.Owner && currentUserRole != UserRole.Admin)
        {
            throw new ForbiddenAccessException("Only Owners and Admins can view pending invitations.");
        }

        // Get the family for the response
        var family = await _unitOfWork.Families.GetByIdAsync(request.FamilyId, cancellationToken)
            ?? throw new NotFoundException("Family", request.FamilyId);

        // Get pending invitations that are still valid (not expired)
        var invitations = await _unitOfWork.Invitations.GetByFamilyIdAndStatusAsync(
            request.FamilyId,
            InvitationStatus.Pending,
            cancellationToken);

        // Filter to only include valid (not expired) invitations
        return invitations
            .Where(i => i.IsValid)
            .Select(invitation => new InvitationDto
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
            })
            .ToList();
    }
}
