using FluentValidation;
using Luminous.Application.Common.Exceptions;
using Luminous.Application.DTOs;
using Luminous.Domain.Interfaces;
using MediatR;

namespace Luminous.Application.Features.Devices.Queries;

/// <summary>
/// Query to get a device by ID.
/// </summary>
public sealed record GetDeviceQuery : IRequest<DeviceDto>
{
    public string DeviceId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
}

/// <summary>
/// Validator for GetDeviceQuery.
/// </summary>
public sealed class GetDeviceQueryValidator : AbstractValidator<GetDeviceQuery>
{
    public GetDeviceQueryValidator()
    {
        RuleFor(x => x.DeviceId)
            .NotEmpty().WithMessage("Device ID is required.");

        RuleFor(x => x.FamilyId)
            .NotEmpty().WithMessage("Family ID is required.");
    }
}

/// <summary>
/// Handler for GetDeviceQuery.
/// </summary>
public sealed class GetDeviceQueryHandler : IRequestHandler<GetDeviceQuery, DeviceDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetDeviceQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DeviceDto> Handle(GetDeviceQuery request, CancellationToken cancellationToken)
    {
        var device = await _unitOfWork.Devices.GetByIdAsync(request.DeviceId, request.FamilyId, cancellationToken)
            ?? throw new NotFoundException("Device", request.DeviceId);

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
