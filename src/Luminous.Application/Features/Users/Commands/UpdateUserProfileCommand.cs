using FluentValidation;
using Luminous.Application.Common.Exceptions;
using Luminous.Application.Common.Interfaces;
using Luminous.Application.DTOs;
using Luminous.Domain.Interfaces;
using Luminous.Domain.ValueObjects;
using MediatR;

namespace Luminous.Application.Features.Users.Commands;

/// <summary>
/// Command to update a user's profile.
/// </summary>
public sealed record UpdateUserProfileCommand : IRequest<UserDto>
{
    /// <summary>
    /// The user ID to update.
    /// </summary>
    public string UserId { get; init; } = string.Empty;

    /// <summary>
    /// The family ID the user belongs to.
    /// </summary>
    public string FamilyId { get; init; } = string.Empty;

    /// <summary>
    /// The updated profile information.
    /// </summary>
    public UserProfileDto Profile { get; init; } = new();
}

/// <summary>
/// Validator for UpdateUserProfileCommand.
/// </summary>
public sealed class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
{
    public UpdateUserProfileCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.FamilyId)
            .NotEmpty().WithMessage("Family ID is required.");

        RuleFor(x => x.Profile.Nickname)
            .MaximumLength(50).WithMessage("Nickname must not exceed 50 characters.")
            .When(x => x.Profile.Nickname != null);

        RuleFor(x => x.Profile.Color)
            .Matches(@"^#[0-9A-Fa-f]{6}$").WithMessage("Color must be a valid hex color code.")
            .When(x => !string.IsNullOrEmpty(x.Profile.Color));
    }
}

/// <summary>
/// Handler for UpdateUserProfileCommand.
/// </summary>
public sealed class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, UserDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITenantContext _tenantContext;

    public UpdateUserProfileCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _tenantContext = tenantContext;
    }

    public async Task<UserDto> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        // Validate tenant access
        _tenantContext.EnsureAccessToFamily(request.FamilyId);

        // Get the user
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, request.FamilyId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("User", request.UserId);
        }

        // Only the user themselves or a family admin can update the profile
        var currentUserId = _currentUserService.UserId;
        var currentUserRole = _currentUserService.Role;

        if (currentUserId != request.UserId &&
            currentUserRole != "Owner" &&
            currentUserRole != "Admin")
        {
            throw new ForbiddenAccessException("You do not have permission to update this profile.");
        }

        // Update the profile
        var profile = new UserProfile
        {
            AvatarUrl = request.Profile.AvatarUrl,
            Color = request.Profile.Color,
            Birthday = request.Profile.Birthday != null ? DateOnly.Parse(request.Profile.Birthday) : null,
            Nickname = request.Profile.Nickname,
            ShowAge = request.Profile.ShowAge
        };

        user.UpdateProfile(profile, _currentUserService.UserId ?? "system");
        await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(user);
    }

    private static UserDto MapToDto(Domain.Entities.User user)
    {
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
