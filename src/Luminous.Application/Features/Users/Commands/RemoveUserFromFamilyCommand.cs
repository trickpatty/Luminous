using FluentValidation;
using Luminous.Application.Common.Exceptions;
using Luminous.Application.Common.Interfaces;
using Luminous.Domain.Enums;
using Luminous.Domain.Interfaces;
using MediatR;

namespace Luminous.Application.Features.Users.Commands;

/// <summary>
/// Command to remove a user from a family.
/// </summary>
public sealed record RemoveUserFromFamilyCommand : IRequest<Unit>
{
    public string FamilyId { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
}

/// <summary>
/// Validator for RemoveUserFromFamilyCommand.
/// </summary>
public sealed class RemoveUserFromFamilyCommandValidator : AbstractValidator<RemoveUserFromFamilyCommand>
{
    public RemoveUserFromFamilyCommandValidator()
    {
        RuleFor(x => x.FamilyId)
            .NotEmpty().WithMessage("Family ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }
}

/// <summary>
/// Handler for RemoveUserFromFamilyCommand.
/// </summary>
public sealed class RemoveUserFromFamilyCommandHandler : IRequestHandler<RemoveUserFromFamilyCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public RemoveUserFromFamilyCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(RemoveUserFromFamilyCommand request, CancellationToken cancellationToken)
    {
        // Verify the current user is authorized (must be Owner or Admin)
        var currentUserRole = Enum.Parse<UserRole>(_currentUserService.Role ?? UserRole.Child.ToString(), true);
        if (currentUserRole != UserRole.Owner && currentUserRole != UserRole.Admin)
        {
            throw new ForbiddenAccessException("Only Owners and Admins can remove users from the family.");
        }

        // Prevent self-removal
        if (_currentUserService.UserId == request.UserId)
        {
            throw new FluentValidation.ValidationException([new FluentValidation.Results.ValidationFailure(
                "UserId", "You cannot remove yourself from the family.")]);
        }

        // Find the target user
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, request.FamilyId, cancellationToken)
            ?? throw new NotFoundException("User", request.UserId);

        // Verify the user belongs to the family
        if (user.FamilyId != request.FamilyId)
        {
            throw new ForbiddenAccessException("You don't have access to this user.");
        }

        // Prevent removing the Owner
        if (user.Role == UserRole.Owner)
        {
            throw new FluentValidation.ValidationException([new FluentValidation.Results.ValidationFailure(
                "UserId", "Cannot remove the family Owner. Transfer ownership first if you want to remove this user.")]);
        }

        // Admin cannot remove another Admin (only Owner can)
        if (currentUserRole == UserRole.Admin && user.Role == UserRole.Admin)
        {
            throw new ForbiddenAccessException("Only the Owner can remove Admins from the family.");
        }

        // Soft delete - deactivate the user instead of hard delete
        user.IsActive = false;
        user.ModifiedAt = DateTime.UtcNow;
        user.ModifiedBy = _currentUserService.UserId;

        await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
