using FluentValidation;
using Luminous.Application.Common.Exceptions;
using Luminous.Application.Common.Interfaces;
using Luminous.Application.DTOs;
using Luminous.Domain.Interfaces;
using Luminous.Domain.ValueObjects;
using MediatR;

namespace Luminous.Application.Features.Families.Commands;

/// <summary>
/// Command to update family settings.
/// </summary>
public sealed record UpdateFamilySettingsCommand : IRequest<FamilyDto>
{
    public string FamilyId { get; init; } = string.Empty;
    public string? Name { get; init; }
    public string? Timezone { get; init; }
    public FamilySettingsDto? Settings { get; init; }
}

/// <summary>
/// Validator for UpdateFamilySettingsCommand.
/// </summary>
public sealed class UpdateFamilySettingsCommandValidator : AbstractValidator<UpdateFamilySettingsCommand>
{
    public UpdateFamilySettingsCommandValidator()
    {
        RuleFor(x => x.FamilyId)
            .NotEmpty().WithMessage("Family ID is required.");

        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Family name must not exceed 100 characters.")
            .When(x => x.Name != null);

        RuleFor(x => x.Settings!.DefaultView)
            .Must(v => v is "day" or "week" or "month" or "agenda")
            .WithMessage("Default view must be 'day', 'week', 'month', or 'agenda'.")
            .When(x => !string.IsNullOrEmpty(x.Settings?.DefaultView));

        RuleFor(x => x.Settings!.PrivacyModeTimeoutMinutes)
            .InclusiveBetween(1, 60)
            .WithMessage("Privacy mode timeout must be between 1 and 60 minutes.")
            .When(x => x.Settings != null);

        RuleFor(x => x.Settings!.WeekStartDay)
            .InclusiveBetween(0, 6)
            .WithMessage("Week start day must be between 0 (Sunday) and 6 (Saturday).")
            .When(x => x.Settings != null);
    }
}

/// <summary>
/// Handler for UpdateFamilySettingsCommand.
/// </summary>
public sealed class UpdateFamilySettingsCommandHandler : IRequestHandler<UpdateFamilySettingsCommand, FamilyDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateFamilySettingsCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<FamilyDto> Handle(UpdateFamilySettingsCommand request, CancellationToken cancellationToken)
    {
        var family = await _unitOfWork.Families.GetByIdAsync(request.FamilyId, cancellationToken)
            ?? throw new NotFoundException("Family", request.FamilyId);

        // Update name if provided
        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            family.Name = request.Name.Trim();
        }

        // Update timezone if provided
        if (!string.IsNullOrWhiteSpace(request.Timezone))
        {
            family.Timezone = request.Timezone;
        }

        // Update settings if provided (merge with existing values)
        if (request.Settings != null)
        {
            var existingSettings = family.Settings ?? new FamilySettings();
            var settings = new FamilySettings
            {
                DefaultView = !string.IsNullOrEmpty(request.Settings.DefaultView)
                    ? request.Settings.DefaultView
                    : existingSettings.DefaultView,
                PrivacyModeEnabled = request.Settings.PrivacyModeEnabled,
                PrivacyModeTimeout = TimeSpan.FromMinutes(request.Settings.PrivacyModeTimeoutMinutes),
                SleepMode = request.Settings.SleepMode != null ? new SleepModeSettings
                {
                    Enabled = request.Settings.SleepMode.Enabled,
                    StartTime = TimeOnly.Parse(request.Settings.SleepMode.StartTime),
                    EndTime = TimeOnly.Parse(request.Settings.SleepMode.EndTime),
                    WakeOnTouch = request.Settings.SleepMode.WakeOnTouch
                } : existingSettings.SleepMode,
                ShowWeather = request.Settings.ShowWeather,
                WeatherLocation = request.Settings.WeatherLocation ?? existingSettings.WeatherLocation,
                UseCelsius = request.Settings.UseCelsius,
                WeekStartDay = request.Settings.WeekStartDay
            };

            family.UpdateSettings(settings, _currentUserService.UserId ?? "system");
        }

        family.ModifiedAt = DateTime.UtcNow;
        family.ModifiedBy = _currentUserService.UserId ?? "system";

        await _unitOfWork.Families.UpdateAsync(family, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(family);
    }

    private static FamilyDto MapToDto(Domain.Entities.Family family)
    {
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
                SleepMode = new SleepModeSettingsDto
                {
                    Enabled = family.Settings.SleepMode.Enabled,
                    StartTime = family.Settings.SleepMode.StartTime.ToString("HH:mm"),
                    EndTime = family.Settings.SleepMode.EndTime.ToString("HH:mm"),
                    WakeOnTouch = family.Settings.SleepMode.WakeOnTouch
                },
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
            CreatedAt = family.CreatedAt
        };
    }
}
