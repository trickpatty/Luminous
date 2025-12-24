using FluentValidation;
using Luminous.Application.DTOs;
using Luminous.Domain.Interfaces;
using MediatR;

namespace Luminous.Application.Features.Devices.Queries;

/// <summary>
/// Query to get all devices for a family.
/// </summary>
public sealed record GetFamilyDevicesQuery : IRequest<IReadOnlyList<DeviceDto>>
{
    public string FamilyId { get; init; } = string.Empty;
    public bool? ActiveOnly { get; init; }
}

/// <summary>
/// Validator for GetFamilyDevicesQuery.
/// </summary>
public sealed class GetFamilyDevicesQueryValidator : AbstractValidator<GetFamilyDevicesQuery>
{
    public GetFamilyDevicesQueryValidator()
    {
        RuleFor(x => x.FamilyId)
            .NotEmpty().WithMessage("Family ID is required.");
    }
}

/// <summary>
/// Handler for GetFamilyDevicesQuery.
/// </summary>
public sealed class GetFamilyDevicesQueryHandler : IRequestHandler<GetFamilyDevicesQuery, IReadOnlyList<DeviceDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetFamilyDevicesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<DeviceDto>> Handle(GetFamilyDevicesQuery request, CancellationToken cancellationToken)
    {
        var devices = await _unitOfWork.Devices.GetByFamilyIdAsync(request.FamilyId, cancellationToken);

        if (request.ActiveOnly == true)
        {
            devices = devices.Where(d => d.IsActive).ToList();
        }

        return devices.Select(device => new DeviceDto
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
        }).ToList();
    }
}
