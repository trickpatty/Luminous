using FluentValidation;
using Luminous.Application.Common.Exceptions;
using Luminous.Application.Common.Interfaces;
using Luminous.Application.DTOs;
using Luminous.Domain.Enums;
using Luminous.Domain.Interfaces;
using Luminous.Domain.ValueObjects;
using MediatR;

namespace Luminous.Application.Features.Users.Commands;

/// <summary>
/// Command to update a user's caregiver information.
/// </summary>
public sealed record UpdateCaregiverInfoCommand : IRequest<UserDto>
{
    public string FamilyId { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public List<string>? Allergies { get; init; }
    public string? MedicalNotes { get; init; }
    public string? EmergencyContactName { get; init; }
    public string? EmergencyContactPhone { get; init; }
    public string? DoctorName { get; init; }
    public string? DoctorPhone { get; init; }
    public string? SchoolName { get; init; }
    public string? Notes { get; init; }
}

/// <summary>
/// Validator for UpdateCaregiverInfoCommand.
/// </summary>
public sealed class UpdateCaregiverInfoCommandValidator : AbstractValidator<UpdateCaregiverInfoCommand>
{
    public UpdateCaregiverInfoCommandValidator()
    {
        RuleFor(x => x.FamilyId)
            .NotEmpty().WithMessage("Family ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.MedicalNotes)
            .MaximumLength(2000).WithMessage("Medical notes must not exceed 2000 characters.");

        RuleFor(x => x.EmergencyContactName)
            .MaximumLength(100).WithMessage("Emergency contact name must not exceed 100 characters.");

        RuleFor(x => x.EmergencyContactPhone)
            .MaximumLength(20).WithMessage("Emergency contact phone must not exceed 20 characters.");

        RuleFor(x => x.DoctorName)
            .MaximumLength(100).WithMessage("Doctor name must not exceed 100 characters.");

        RuleFor(x => x.DoctorPhone)
            .MaximumLength(20).WithMessage("Doctor phone must not exceed 20 characters.");

        RuleFor(x => x.SchoolName)
            .MaximumLength(100).WithMessage("School name must not exceed 100 characters.");

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes must not exceed 2000 characters.");

        RuleForEach(x => x.Allergies)
            .MaximumLength(100).WithMessage("Each allergy must not exceed 100 characters.");
    }
}

/// <summary>
/// Handler for UpdateCaregiverInfoCommand.
/// </summary>
public sealed class UpdateCaregiverInfoCommandHandler : IRequestHandler<UpdateCaregiverInfoCommand, UserDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateCaregiverInfoCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<UserDto> Handle(UpdateCaregiverInfoCommand request, CancellationToken cancellationToken)
    {
        // Get current user role
        var currentUserRole = Enum.Parse<UserRole>(_currentUserService.Role ?? UserRole.Child.ToString(), true);

        // Find the target user
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, request.FamilyId, cancellationToken)
            ?? throw new NotFoundException("User", request.UserId);

        // Verify the user belongs to the family
        if (user.FamilyId != request.FamilyId)
        {
            throw new ForbiddenAccessException("You don't have access to this user.");
        }

        // Authorization: User can update their own info, or Owner/Admin can update anyone's
        var isOwnProfile = _currentUserService.UserId == request.UserId;
        var isAdminOrOwner = currentUserRole == UserRole.Owner || currentUserRole == UserRole.Admin;

        if (!isOwnProfile && !isAdminOrOwner)
        {
            throw new ForbiddenAccessException("You can only update your own caregiver information.");
        }

        // Update caregiver info
        user.CaregiverInfo = new CaregiverInfo
        {
            Allergies = request.Allergies ?? [],
            MedicalNotes = request.MedicalNotes,
            EmergencyContactName = request.EmergencyContactName,
            EmergencyContactPhone = request.EmergencyContactPhone,
            DoctorName = request.DoctorName,
            DoctorPhone = request.DoctorPhone,
            SchoolName = request.SchoolName,
            Notes = request.Notes
        };

        user.ModifiedAt = DateTime.UtcNow;
        user.ModifiedBy = _currentUserService.UserId;

        await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
