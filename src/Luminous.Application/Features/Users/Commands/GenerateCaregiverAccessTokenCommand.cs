using FluentValidation;
using Luminous.Application.Common.Exceptions;
using Luminous.Application.Common.Interfaces;
using Luminous.Application.DTOs;
using Luminous.Domain.Enums;
using Luminous.Domain.Interfaces;
using MediatR;

namespace Luminous.Application.Features.Users.Commands;

/// <summary>
/// Command to generate a time-limited caregiver access token.
/// </summary>
public sealed record GenerateCaregiverAccessTokenCommand : IRequest<CaregiverAccessTokenDto>
{
    public string FamilyId { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public int ExpirationHours { get; init; } = 24;
}

/// <summary>
/// Validator for GenerateCaregiverAccessTokenCommand.
/// </summary>
public sealed class GenerateCaregiverAccessTokenCommandValidator : AbstractValidator<GenerateCaregiverAccessTokenCommand>
{
    public GenerateCaregiverAccessTokenCommandValidator()
    {
        RuleFor(x => x.FamilyId)
            .NotEmpty().WithMessage("Family ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.ExpirationHours)
            .InclusiveBetween(1, 168).WithMessage("Expiration must be between 1 and 168 hours (1 week).");
    }
}

/// <summary>
/// Handler for GenerateCaregiverAccessTokenCommand.
/// </summary>
public sealed class GenerateCaregiverAccessTokenCommandHandler : IRequestHandler<GenerateCaregiverAccessTokenCommand, CaregiverAccessTokenDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITokenService _tokenService;

    public GenerateCaregiverAccessTokenCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ITokenService tokenService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _tokenService = tokenService;
    }

    public async Task<CaregiverAccessTokenDto> Handle(GenerateCaregiverAccessTokenCommand request, CancellationToken cancellationToken)
    {
        // Verify the current user is authorized (must be Owner or Admin)
        var currentUserRole = Enum.Parse<UserRole>(_currentUserService.Role ?? UserRole.Child.ToString(), true);
        if (currentUserRole != UserRole.Owner && currentUserRole != UserRole.Admin)
        {
            throw new ForbiddenAccessException("Only Owners and Admins can generate caregiver access tokens.");
        }

        // Verify family exists
        var family = await _unitOfWork.Families.GetByIdAsync(request.FamilyId, cancellationToken)
            ?? throw new NotFoundException("Family", request.FamilyId);

        // Find the target user (the one whose info the caregiver will access)
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, request.FamilyId, cancellationToken)
            ?? throw new NotFoundException("User", request.UserId);

        // Verify the user belongs to the family
        if (user.FamilyId != request.FamilyId)
        {
            throw new ForbiddenAccessException("You don't have access to this user.");
        }

        // Generate the caregiver token
        var expiration = TimeSpan.FromHours(request.ExpirationHours);
        var token = _tokenService.GenerateCaregiverToken(request.FamilyId, request.UserId, expiration);

        return token;
    }
}
