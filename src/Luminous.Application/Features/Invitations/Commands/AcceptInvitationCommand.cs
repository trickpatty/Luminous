using FluentValidation;
using Luminous.Application.Common.Exceptions;
using Luminous.Application.Common.Interfaces;
using Luminous.Application.DTOs;
using Luminous.Domain.Entities;
using Luminous.Domain.Enums;
using Luminous.Domain.Interfaces;
using Luminous.Domain.ValueObjects;
using MediatR;

namespace Luminous.Application.Features.Invitations.Commands;

/// <summary>
/// Command to accept a family invitation.
/// </summary>
public sealed record AcceptInvitationCommand : IRequest<AcceptedInvitationResultDto>
{
    public string Code { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string? Nickname { get; init; }
    public string? AvatarUrl { get; init; }
    public string? Color { get; init; }
}

/// <summary>
/// Validator for AcceptInvitationCommand.
/// </summary>
public sealed class AcceptInvitationCommandValidator : AbstractValidator<AcceptInvitationCommand>
{
    public AcceptInvitationCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Invitation code is required.");

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required.")
            .MaximumLength(50).WithMessage("Display name must not exceed 50 characters.")
            .Matches(@"^[\w\s\-'\.]+$").WithMessage("Display name contains invalid characters.");

        RuleFor(x => x.Nickname)
            .MaximumLength(30).WithMessage("Nickname must not exceed 30 characters.");

        RuleFor(x => x.AvatarUrl)
            .MaximumLength(500).WithMessage("Avatar URL must not exceed 500 characters.")
            .Must(url => url == null || Uri.TryCreate(url, UriKind.Absolute, out _))
            .WithMessage("Avatar URL must be a valid URL.");

        RuleFor(x => x.Color)
            .Matches(@"^#[0-9A-Fa-f]{6}$")
            .When(x => x.Color != null)
            .WithMessage("Color must be a valid hex color code (e.g., #3B82F6).");
    }
}

/// <summary>
/// Handler for AcceptInvitationCommand.
/// </summary>
public sealed class AcceptInvitationCommandHandler : IRequestHandler<AcceptInvitationCommand, AcceptedInvitationResultDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;

    public AcceptInvitationCommandHandler(
        IUnitOfWork unitOfWork,
        ITokenService tokenService)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
    }

    public async Task<AcceptedInvitationResultDto> Handle(AcceptInvitationCommand request, CancellationToken cancellationToken)
    {
        // Find the invitation by code
        var invitation = await _unitOfWork.Invitations.GetByCodeAsync(request.Code, cancellationToken)
            ?? throw new NotFoundException("Invitation", request.Code);

        // Verify the invitation is still valid
        if (!invitation.IsValid)
        {
            var message = invitation.Status switch
            {
                InvitationStatus.Accepted => "This invitation has already been accepted.",
                InvitationStatus.Declined => "This invitation has been declined.",
                InvitationStatus.Revoked => "This invitation has been revoked.",
                InvitationStatus.Expired => "This invitation has expired.",
                _ => invitation.ExpiresAt <= DateTime.UtcNow
                    ? "This invitation has expired."
                    : "This invitation is no longer valid."
            };
            throw new FluentValidation.ValidationException([new FluentValidation.Results.ValidationFailure(
                "Code", message)]);
        }

        // Verify family still exists
        var family = await _unitOfWork.Families.GetByIdAsync(invitation.FamilyId, cancellationToken)
            ?? throw new NotFoundException("Family", invitation.FamilyId);

        // Check if email is already registered as a user in this family
        var existingUsers = await _unitOfWork.Users.GetByFamilyIdAsync(invitation.FamilyId, cancellationToken);
        if (existingUsers.Any(u => u.Email.Equals(invitation.Email, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ConflictException($"A user with email '{invitation.Email}' is already a member of this family.");
        }

        // Create the new user
        var user = User.Create(
            invitation.FamilyId,
            invitation.Email,
            request.DisplayName,
            invitation.Role);

        // Set up profile
        user.UpdateProfile(new UserProfile
        {
            AvatarUrl = request.AvatarUrl,
            Color = request.Color ?? "#3B82F6",
            Nickname = request.Nickname
        });

        // Mark email as verified (they received the invitation email)
        user.EmailVerified = true;
        user.RecordLogin();

        await _unitOfWork.Users.AddAsync(user, cancellationToken);

        // Accept the invitation
        invitation.Accept(user.Id);
        await _unitOfWork.Invitations.UpdateAsync(invitation, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Generate authentication token
        var authResult = _tokenService.GenerateToken(user);

        var userDto = new UserDto
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

        return new AcceptedInvitationResultDto
        {
            User = userDto,
            AccessToken = authResult.AccessToken,
            RefreshToken = authResult.RefreshToken,
            TokenType = authResult.TokenType,
            ExpiresIn = authResult.ExpiresIn
        };
    }
}
