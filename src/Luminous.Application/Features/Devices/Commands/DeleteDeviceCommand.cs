using FluentValidation;
using Luminous.Application.Common.Exceptions;
using Luminous.Domain.Interfaces;
using MediatR;

namespace Luminous.Application.Features.Devices.Commands;

/// <summary>
/// Command to delete a device.
/// </summary>
public sealed record DeleteDeviceCommand : IRequest<Unit>
{
    public string DeviceId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
}

/// <summary>
/// Validator for DeleteDeviceCommand.
/// </summary>
public sealed class DeleteDeviceCommandValidator : AbstractValidator<DeleteDeviceCommand>
{
    public DeleteDeviceCommandValidator()
    {
        RuleFor(x => x.DeviceId)
            .NotEmpty().WithMessage("Device ID is required.");

        RuleFor(x => x.FamilyId)
            .NotEmpty().WithMessage("Family ID is required.");
    }
}

/// <summary>
/// Handler for DeleteDeviceCommand.
/// </summary>
public sealed class DeleteDeviceCommandHandler : IRequestHandler<DeleteDeviceCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteDeviceCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteDeviceCommand request, CancellationToken cancellationToken)
    {
        var device = await _unitOfWork.Devices.GetByIdAsync(request.DeviceId, request.FamilyId, cancellationToken)
            ?? throw new NotFoundException("Device", request.DeviceId);

        await _unitOfWork.Devices.DeleteAsync(device, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
