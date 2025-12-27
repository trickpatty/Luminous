using FluentValidation;
using Luminous.Application.Common.Exceptions;
using Luminous.Application.Common.Interfaces;
using Luminous.Application.DTOs;
using Luminous.Domain.Entities;
using Luminous.Domain.Enums;
using Luminous.Domain.Interfaces;
using MediatR;

namespace Luminous.Application.Features.Invitations.Commands;

/// <summary>
/// Command to create and send a family invitation.
/// </summary>
public sealed record CreateInvitationCommand : IRequest<InvitationDto>
{
    public string FamilyId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public UserRole Role { get; init; } = UserRole.Adult;
    public string? Message { get; init; }
}

/// <summary>
/// Validator for CreateInvitationCommand.
/// </summary>
public sealed class CreateInvitationCommandValidator : AbstractValidator<CreateInvitationCommand>
{
    public CreateInvitationCommandValidator()
    {
        RuleFor(x => x.FamilyId)
            .NotEmpty().WithMessage("Family ID is required.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters.");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Invalid role specified.")
            .Must(r => r != UserRole.Owner).WithMessage("Cannot invite someone as Owner.");

        RuleFor(x => x.Message)
            .MaximumLength(500).WithMessage("Message must not exceed 500 characters.");
    }
}

/// <summary>
/// Handler for CreateInvitationCommand.
/// </summary>
public sealed class CreateInvitationCommandHandler : IRequestHandler<CreateInvitationCommand, InvitationDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailService _emailService;

    public CreateInvitationCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _emailService = emailService;
    }

    public async Task<InvitationDto> Handle(CreateInvitationCommand request, CancellationToken cancellationToken)
    {
        // Verify family exists and user has access
        var family = await _unitOfWork.Families.GetByIdAsync(request.FamilyId, cancellationToken)
            ?? throw new NotFoundException("Family", request.FamilyId);

        // Check if the user is authorized (must be Owner or Admin)
        var currentUserRole = Enum.Parse<UserRole>(_currentUserService.Role ?? UserRole.Child.ToString(), true);
        if (currentUserRole != UserRole.Owner && currentUserRole != UserRole.Admin)
        {
            throw new ForbiddenAccessException("Only Owners and Admins can send invitations.");
        }

        // Normalize email
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        // Check if email is already a member of this family
        var existingUsers = await _unitOfWork.Users.GetByFamilyIdAsync(request.FamilyId, cancellationToken);
        if (existingUsers.Any(u => u.Email.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ConflictException($"A user with email '{request.Email}' is already a member of this family.");
        }

        // Check for existing pending invitation
        if (await _unitOfWork.Invitations.HasPendingInvitationAsync(normalizedEmail, request.FamilyId, cancellationToken))
        {
            throw new ConflictException($"A pending invitation for '{request.Email}' already exists.");
        }

        // Create the invitation
        var invitation = Invitation.Create(
            request.FamilyId,
            normalizedEmail,
            request.Role,
            _currentUserService.UserId ?? "system",
            expirationDays: 7,
            request.Message);

        await _unitOfWork.Invitations.AddAsync(invitation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Get inviter's display name
        var inviter = await _unitOfWork.Users.GetByIdAsync(
            _currentUserService.UserId ?? "system",
            request.FamilyId,
            cancellationToken);
        var inviterName = inviter?.DisplayName ?? "A family member";

        // Send invitation email
        await _emailService.SendInvitationAsync(
            invitation.Email,
            family.Name,
            inviterName,
            invitation.Code,
            cancellationToken);

        return new InvitationDto
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
        };
    }
}
