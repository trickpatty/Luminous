using Luminous.Application.Common.Exceptions;
using Luminous.Application.Common.Interfaces;
using Luminous.Application.DTOs;
using Luminous.Domain.Interfaces;
using MediatR;

namespace Luminous.Application.Features.Users.Queries;

/// <summary>
/// Query to get the currently authenticated user's information.
/// </summary>
public sealed record GetCurrentUserQuery : IRequest<UserDto>;

/// <summary>
/// Handler for GetCurrentUserQuery.
/// </summary>
public sealed class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, UserDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetCurrentUserQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<UserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User ID not found in claims.");

        var familyId = _currentUserService.FamilyId
            ?? throw new UnauthorizedAccessException("Family ID not found in claims.");

        var user = await _unitOfWork.Users.GetByIdAsync(userId, familyId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("User", userId);
        }

        return new UserDto
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
        };
    }
}
