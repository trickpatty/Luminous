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
public sealed record UpdateFamilySettingsCommand : IRequest<FamilySettingsDto>
{
    public string FamilyId { get; init; } = string.Empty;
    public FamilySettingsDto Settings { get; init; } = new();
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

        RuleFor(x => x.Settings.DefaultView)
            .Must(v => v is "day" or "week" or "month" or "agenda")
            .WithMessage("Default view must be 'day', 'week', 'month', or 'agenda'.");

        RuleFor(x => x.Settings.PrivacyModeTimeoutMinutes)
            .InclusiveBetween(1, 60)
            .WithMessage("Privacy mode timeout must be between 1 and 60 minutes.");

        RuleFor(x => x.Settings.WeekStartDay)
            .InclusiveBetween(0, 6)
            .WithMessage("Week start day must be between 0 (Sunday) and 6 (Saturday).");
    }
}

/// <summary>
/// Handler for UpdateFamilySettingsCommand.
/// </summary>
public sealed class UpdateFamilySettingsCommandHandler : IRequestHandler<UpdateFamilySettingsCommand, FamilySettingsDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateFamilySettingsCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<FamilySettingsDto> Handle(UpdateFamilySettingsCommand request, CancellationToken cancellationToken)
    {
        var family = await _unitOfWork.Families.GetByIdAsync(request.FamilyId, cancellationToken)
            ?? throw new NotFoundException("Family", request.FamilyId);

        var settings = new FamilySettings
        {
            DefaultView = request.Settings.DefaultView,
            PrivacyModeEnabled = request.Settings.PrivacyModeEnabled,
            PrivacyModeTimeout = TimeSpan.FromMinutes(request.Settings.PrivacyModeTimeoutMinutes),
            SleepMode = new SleepModeSettings
            {
                Enabled = request.Settings.SleepMode.Enabled,
                StartTime = TimeOnly.Parse(request.Settings.SleepMode.StartTime),
                EndTime = TimeOnly.Parse(request.Settings.SleepMode.EndTime),
                WakeOnTouch = request.Settings.SleepMode.WakeOnTouch
            },
            ShowWeather = request.Settings.ShowWeather,
            WeatherLocation = request.Settings.WeatherLocation,
            UseCelsius = request.Settings.UseCelsius,
            WeekStartDay = request.Settings.WeekStartDay
        };

        family.UpdateSettings(settings, _currentUserService.UserId ?? "system");
        await _unitOfWork.Families.UpdateAsync(family, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return request.Settings;
    }
}
