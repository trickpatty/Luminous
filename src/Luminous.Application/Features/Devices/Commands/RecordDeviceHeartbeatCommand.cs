using FluentValidation;
using Luminous.Application.Common.Exceptions;
using Luminous.Application.DTOs;
using Luminous.Domain.Interfaces;
using MediatR;

namespace Luminous.Application.Features.Devices.Commands;

/// <summary>
/// Command to record a device heartbeat without requiring family context.
/// Used by display app for both linked and unlinked devices.
/// </summary>
public sealed record RecordDeviceHeartbeatCommand : IRequest<DeviceHeartbeatDto>
{
    public string DeviceId { get; init; } = string.Empty;
    public string? AppVersion { get; init; }
}

/// <summary>
/// Validator for RecordDeviceHeartbeatCommand.
/// </summary>
public sealed class RecordDeviceHeartbeatCommandValidator : AbstractValidator<RecordDeviceHeartbeatCommand>
{
    public RecordDeviceHeartbeatCommandValidator()
    {
        RuleFor(x => x.DeviceId)
            .NotEmpty().WithMessage("Device ID is required.");

        When(x => x.AppVersion != null, () =>
        {
            RuleFor(x => x.AppVersion)
                .MaximumLength(50).WithMessage("App version must not exceed 50 characters.");
        });
    }
}

/// <summary>
/// Handler for RecordDeviceHeartbeatCommand.
/// </summary>
public sealed class RecordDeviceHeartbeatCommandHandler : IRequestHandler<RecordDeviceHeartbeatCommand, DeviceHeartbeatDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public RecordDeviceHeartbeatCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DeviceHeartbeatDto> Handle(RecordDeviceHeartbeatCommand request, CancellationToken cancellationToken)
    {
        // For unlinked devices, the partition key is the device ID
        var device = await _unitOfWork.Devices.GetByIdAsync(request.DeviceId, request.DeviceId, cancellationToken);

        if (device == null)
        {
            throw new NotFoundException("Device", request.DeviceId);
        }

        device.RecordHeartbeat(request.AppVersion);

        await _unitOfWork.Devices.UpdateAsync(device, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DeviceHeartbeatDto
        {
            DeviceId = device.Id,
            LastSeenAt = device.LastSeenAt,
            IsActive = device.IsActive,
            AppVersion = device.AppVersion
        };
    }
}
