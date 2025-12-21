using FluentValidation;
using Luminous.Application.Common.Exceptions;
using Luminous.Application.DTOs;
using Luminous.Domain.Interfaces;
using MediatR;

namespace Luminous.Application.Features.Families.Queries;

/// <summary>
/// Query to get a family by ID.
/// </summary>
public sealed record GetFamilyQuery : IRequest<FamilyDto>
{
    public string FamilyId { get; init; } = string.Empty;
}

/// <summary>
/// Validator for GetFamilyQuery.
/// </summary>
public sealed class GetFamilyQueryValidator : AbstractValidator<GetFamilyQuery>
{
    public GetFamilyQueryValidator()
    {
        RuleFor(x => x.FamilyId)
            .NotEmpty().WithMessage("Family ID is required.");
    }
}

/// <summary>
/// Handler for GetFamilyQuery.
/// </summary>
public sealed class GetFamilyQueryHandler : IRequestHandler<GetFamilyQuery, FamilyDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetFamilyQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<FamilyDto> Handle(GetFamilyQuery request, CancellationToken cancellationToken)
    {
        var family = await _unitOfWork.Families.GetByIdAsync(request.FamilyId, cancellationToken)
            ?? throw new NotFoundException("Family", request.FamilyId);

        var members = await _unitOfWork.Users.GetByFamilyIdAsync(request.FamilyId, cancellationToken);
        var devices = await _unitOfWork.Devices.GetByFamilyIdAsync(request.FamilyId, cancellationToken);

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
            CreatedAt = family.CreatedAt,
            MemberCount = members.Count,
            DeviceCount = devices.Count(d => d.IsLinked)
        };
    }
}
