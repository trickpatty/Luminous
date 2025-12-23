using FluentValidation;
using Luminous.Application.Common.Exceptions;
using Luminous.Application.DTOs;
using Luminous.Domain.Interfaces;
using MediatR;

namespace Luminous.Application.Features.Devices.Commands;

/// <summary>
/// Command to record a device heartbeat (used for tracking device online status).
/// </summary>
public sealed record RecordHeartbeatCommand : IRequest<DeviceHeartbeatDto>
{
    public string DeviceId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string? AppVersion { get; init; }
}

/// <summary>
/// Validator for RecordHeartbeatCommand.
/// </summary>
public sealed class RecordHeartbeatCommandValidator : AbstractValidator<RecordHeartbeatCommand>
{
    public RecordHeartbeatCommandValidator()
    {
        RuleFor(x => x.DeviceId)
            .NotEmpty().WithMessage("Device ID is required.");

        RuleFor(x => x.FamilyId)
            .NotEmpty().WithMessage("Family ID is required.");

        When(x => x.AppVersion != null, () =>
        {
            RuleFor(x => x.AppVersion)
                .MaximumLength(50).WithMessage("App version must not exceed 50 characters.");
        });
    }
}

/// <summary>
/// Handler for RecordHeartbeatCommand.
/// </summary>
public sealed class RecordHeartbeatCommandHandler : IRequestHandler<RecordHeartbeatCommand, DeviceHeartbeatDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public RecordHeartbeatCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DeviceHeartbeatDto> Handle(RecordHeartbeatCommand request, CancellationToken cancellationToken)
    {
        var device = await _unitOfWork.Devices.GetByIdAsync(request.DeviceId, request.FamilyId, cancellationToken)
            ?? throw new NotFoundException("Device", request.DeviceId);

        if (!device.IsLinked)
        {
            throw new ValidationException([new FluentValidation.Results.ValidationFailure(
                "DeviceId", "Device is not linked to any family.")]);
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
