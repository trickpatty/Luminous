using FluentValidation;
using Luminous.Application.Common.Exceptions;
using Luminous.Application.Common.Interfaces;
using Luminous.Application.DTOs;
using Luminous.Domain.Enums;
using Luminous.Domain.Interfaces;
using MediatR;

namespace Luminous.Application.Features.Users.Commands;

/// <summary>
/// Command to update a user's role within the family.
/// </summary>
public sealed record UpdateUserRoleCommand : IRequest<UserDto>
{
    public string FamilyId { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public UserRole NewRole { get; init; }
}

/// <summary>
/// Validator for UpdateUserRoleCommand.
/// </summary>
public sealed class UpdateUserRoleCommandValidator : AbstractValidator<UpdateUserRoleCommand>
{
    public UpdateUserRoleCommandValidator()
    {
        RuleFor(x => x.FamilyId)
            .NotEmpty().WithMessage("Family ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.NewRole)
            .IsInEnum().WithMessage("Invalid role specified.")
            .Must(r => r != UserRole.Owner).WithMessage("Cannot assign Owner role through this endpoint. Use transfer ownership instead.");
    }
}

/// <summary>
/// Handler for UpdateUserRoleCommand.
/// </summary>
public sealed class UpdateUserRoleCommandHandler : IRequestHandler<UpdateUserRoleCommand, UserDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateUserRoleCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<UserDto> Handle(UpdateUserRoleCommand request, CancellationToken cancellationToken)
    {
        // Verify the current user is authorized (must be Owner or Admin)
        var currentUserRole = Enum.Parse<UserRole>(_currentUserService.Role ?? UserRole.Child.ToString(), true);
        if (currentUserRole != UserRole.Owner && currentUserRole != UserRole.Admin)
        {
            throw new ForbiddenAccessException("Only Owners and Admins can update user roles.");
        }

        // Find the target user
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, request.FamilyId, cancellationToken)
            ?? throw new NotFoundException("User", request.UserId);

        // Verify the user belongs to the family
        if (user.FamilyId != request.FamilyId)
        {
            throw new ForbiddenAccessException("You don't have access to this user.");
        }

        // Prevent changing the role of an Owner
        if (user.Role == UserRole.Owner)
        {
            throw new FluentValidation.ValidationException([new FluentValidation.Results.ValidationFailure(
                "UserId", "Cannot change the role of the family Owner. Use transfer ownership instead.")]);
        }

        // Admin cannot promote someone to Admin (only Owner can)
        if (currentUserRole == UserRole.Admin && request.NewRole == UserRole.Admin)
        {
            throw new ForbiddenAccessException("Only the Owner can promote users to Admin.");
        }

        // Prevent self-demotion if current user is an Admin
        if (_currentUserService.UserId == request.UserId && request.NewRole != UserRole.Admin)
        {
            throw new FluentValidation.ValidationException([new FluentValidation.Results.ValidationFailure(
                "UserId", "You cannot demote yourself. Ask another Admin or the Owner.")]);
        }

        // Update the role
        user.Role = request.NewRole;
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
