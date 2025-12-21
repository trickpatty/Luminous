using FluentValidation;
using Luminous.Application.Common.Exceptions;
using Luminous.Application.Common.Interfaces;
using Luminous.Application.DTOs;
using Luminous.Domain.Entities;
using Luminous.Domain.Enums;
using Luminous.Domain.Interfaces;
using MediatR;

namespace Luminous.Application.Features.Families.Commands;

/// <summary>
/// Command to create a new family.
/// </summary>
public sealed record CreateFamilyCommand : IRequest<FamilyDto>
{
    public string Name { get; init; } = string.Empty;
    public string Timezone { get; init; } = "UTC";
    public string OwnerEmail { get; init; } = string.Empty;
    public string OwnerDisplayName { get; init; } = string.Empty;
}

/// <summary>
/// Validator for CreateFamilyCommand.
/// </summary>
public sealed class CreateFamilyCommandValidator : AbstractValidator<CreateFamilyCommand>
{
    public CreateFamilyCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Family name is required.")
            .MaximumLength(100).WithMessage("Family name must not exceed 100 characters.");

        RuleFor(x => x.Timezone)
            .NotEmpty().WithMessage("Timezone is required.");

        RuleFor(x => x.OwnerEmail)
            .NotEmpty().WithMessage("Owner email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.OwnerDisplayName)
            .NotEmpty().WithMessage("Owner display name is required.")
            .MaximumLength(50).WithMessage("Display name must not exceed 50 characters.");
    }
}

/// <summary>
/// Handler for CreateFamilyCommand.
/// </summary>
public sealed class CreateFamilyCommandHandler : IRequestHandler<CreateFamilyCommand, FamilyDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeService _dateTimeService;

    public CreateFamilyCommandHandler(IUnitOfWork unitOfWork, IDateTimeService dateTimeService)
    {
        _unitOfWork = unitOfWork;
        _dateTimeService = dateTimeService;
    }

    public async Task<FamilyDto> Handle(CreateFamilyCommand request, CancellationToken cancellationToken)
    {
        // Check if email is already in use
        if (await _unitOfWork.Users.EmailExistsAsync(request.OwnerEmail, cancellationToken))
        {
            throw new ConflictException($"Email {request.OwnerEmail} is already in use.");
        }

        // Create the family
        var family = Family.Create(request.Name, request.Timezone, request.OwnerEmail);
        await _unitOfWork.Families.AddAsync(family, cancellationToken);

        // Create the owner user
        var owner = User.CreateOwner(
            family.Id,
            request.OwnerEmail,
            request.OwnerDisplayName);
        await _unitOfWork.Users.AddAsync(owner, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new FamilyDto
        {
            Id = family.Id,
            Name = family.Name,
            Timezone = family.Timezone,
            Settings = new FamilySettingsDto
            {
                DefaultView = family.Settings.DefaultView,
                PrivacyModeEnabled = family.Settings.PrivacyModeEnabled,
                PrivacyModeTimeoutMinutes = (int)family.Settings.PrivacyModeTimeout.TotalMinutes,
                ShowWeather = family.Settings.ShowWeather,
                WeatherLocation = family.Settings.WeatherLocation,
                UseCelsius = family.Settings.UseCelsius,
                WeekStartDay = family.Settings.WeekStartDay
            },
            Subscription = family.Subscription != null ? new SubscriptionDto
            {
                Tier = family.Subscription.Tier,
                StartedAt = family.Subscription.StartedAt,
                ExpiresAt = family.Subscription.ExpiresAt,
                IsActive = family.Subscription.IsActive
            } : null,
            CreatedAt = family.CreatedAt,
            MemberCount = 1,
            DeviceCount = 0
        };
    }
}
