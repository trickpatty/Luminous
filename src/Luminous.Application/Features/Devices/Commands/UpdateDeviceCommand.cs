using FluentValidation;
using Luminous.Application.Common.Exceptions;
using Luminous.Application.Common.Interfaces;
using Luminous.Application.DTOs;
using Luminous.Domain.Interfaces;
using Luminous.Domain.ValueObjects;
using MediatR;

namespace Luminous.Application.Features.Devices.Commands;

/// <summary>
/// Command to update a device's name and/or settings.
/// </summary>
public sealed record UpdateDeviceCommand : IRequest<DeviceDto>
{
    public string DeviceId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string? Name { get; init; }
    public DeviceSettingsDto? Settings { get; init; }
}

/// <summary>
/// Validator for UpdateDeviceCommand.
/// </summary>
public sealed class UpdateDeviceCommandValidator : AbstractValidator<UpdateDeviceCommand>
{
    public UpdateDeviceCommandValidator()
    {
        RuleFor(x => x.DeviceId)
            .NotEmpty().WithMessage("Device ID is required.");

        RuleFor(x => x.FamilyId)
            .NotEmpty().WithMessage("Family ID is required.");

        When(x => x.Name != null, () =>
        {
            RuleFor(x => x.Name)
                .MaximumLength(50).WithMessage("Device name must not exceed 50 characters.");
        });

        When(x => x.Settings != null, () =>
        {
            RuleFor(x => x.Settings!.Brightness)
                .InclusiveBetween(0, 100).WithMessage("Brightness must be between 0 and 100.");

            RuleFor(x => x.Settings!.Volume)
                .InclusiveBetween(0, 100).WithMessage("Volume must be between 0 and 100.");

            RuleFor(x => x.Settings!.Orientation)
                .Must(o => o == "portrait" || o == "landscape")
                .WithMessage("Orientation must be 'portrait' or 'landscape'.");

            RuleFor(x => x.Settings!.DefaultView)
                .Must(v => new[] { "day", "week", "month", "agenda" }.Contains(v))
                .WithMessage("Default view must be 'day', 'week', 'month', or 'agenda'.");
        });
    }
}

/// <summary>
/// Handler for UpdateDeviceCommand.
/// </summary>
public sealed class UpdateDeviceCommandHandler : IRequestHandler<UpdateDeviceCommand, DeviceDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateDeviceCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<DeviceDto> Handle(UpdateDeviceCommand request, CancellationToken cancellationToken)
    {
        var device = await _unitOfWork.Devices.GetByIdAsync(request.DeviceId, request.FamilyId, cancellationToken)
            ?? throw new NotFoundException("Device", request.DeviceId);

        var modifiedBy = _currentUserService.UserId ?? "system";

        // Update name if provided
        if (!string.IsNullOrEmpty(request.Name))
        {
            device.Rename(request.Name, modifiedBy);
        }

        // Update settings if provided
        if (request.Settings != null)
        {
            var settings = new DeviceSettings
            {
                DefaultView = request.Settings.DefaultView,
                Brightness = request.Settings.Brightness,
                AutoBrightness = request.Settings.AutoBrightness,
                Orientation = request.Settings.Orientation,
                SoundEnabled = request.Settings.SoundEnabled,
                Volume = request.Settings.Volume
            };
            device.UpdateSettings(settings, modifiedBy);
        }

        await _unitOfWork.Devices.UpdateAsync(device, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DeviceDto
        {
            Id = device.Id,
            FamilyId = device.FamilyId,
            Type = device.Type,
            Name = device.Name,
            IsLinked = device.IsLinked,
            LinkedAt = device.LinkedAt,
            LinkedBy = device.LinkedBy,
            Settings = new DeviceSettingsDto
            {
                DefaultView = device.Settings.DefaultView,
                Brightness = device.Settings.Brightness,
                AutoBrightness = device.Settings.AutoBrightness,
                Orientation = device.Settings.Orientation,
                SoundEnabled = device.Settings.SoundEnabled,
                Volume = device.Settings.Volume
            },
            LastSeenAt = device.LastSeenAt,
            IsActive = device.IsActive,
            Platform = device.Platform,
            AppVersion = device.AppVersion
        };
    }
}
