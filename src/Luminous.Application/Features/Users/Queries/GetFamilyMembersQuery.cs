using FluentValidation;
using Luminous.Application.DTOs;
using Luminous.Domain.Interfaces;
using MediatR;

namespace Luminous.Application.Features.Users.Queries;

/// <summary>
/// Query to get all members of a family.
/// </summary>
public sealed record GetFamilyMembersQuery : IRequest<IReadOnlyList<UserDto>>
{
    public string FamilyId { get; init; } = string.Empty;
}

/// <summary>
/// Validator for GetFamilyMembersQuery.
/// </summary>
public sealed class GetFamilyMembersQueryValidator : AbstractValidator<GetFamilyMembersQuery>
{
    public GetFamilyMembersQueryValidator()
    {
        RuleFor(x => x.FamilyId)
            .NotEmpty().WithMessage("Family ID is required.");
    }
}

/// <summary>
/// Handler for GetFamilyMembersQuery.
/// </summary>
public sealed class GetFamilyMembersQueryHandler : IRequestHandler<GetFamilyMembersQuery, IReadOnlyList<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetFamilyMembersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<UserDto>> Handle(GetFamilyMembersQuery request, CancellationToken cancellationToken)
    {
        var members = await _unitOfWork.Users.GetByFamilyIdAsync(request.FamilyId, cancellationToken);

        return members.Select(user => new UserDto
        {
            Id = user.Id,
            FamilyId = user.FamilyId,
            Email = user.Email,
            DisplayName = user.DisplayName,
            Role = user.Role,
            Profile = new UserProfileDto
            {
                AvatarUrl = user.Profile.AvatarUrl,
                Color = user.Profile.Color,
                Birthday = user.Profile.Birthday?.ToString("yyyy-MM-dd"),
                Nickname = user.Profile.Nickname,
                ShowAge = user.Profile.ShowAge,
                Age = user.Profile.Age
            },
            CaregiverInfo = user.CaregiverInfo != null ? new CaregiverInfoDto
            {
                Allergies = user.CaregiverInfo.Allergies,
                MedicalNotes = user.CaregiverInfo.MedicalNotes,
                EmergencyContactName = user.CaregiverInfo.EmergencyContactName,
                EmergencyContactPhone = user.CaregiverInfo.EmergencyContactPhone,
                DoctorName = user.CaregiverInfo.DoctorName,
                DoctorPhone = user.CaregiverInfo.DoctorPhone,
                SchoolName = user.CaregiverInfo.SchoolName,
                Notes = user.CaregiverInfo.Notes
            } : null,
            EmailVerified = user.EmailVerified,
            LastLoginAt = user.LastLoginAt,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        }).ToList();
    }
}
