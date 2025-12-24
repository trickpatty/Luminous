using FluentValidation;
using Luminous.Application.Common.Exceptions;
using Luminous.Application.Common.Interfaces;
using Luminous.Domain.Interfaces;
using MediatR;

namespace Luminous.Application.Features.Devices.Commands;

/// <summary>
/// Command to unlink a device from a family.
/// </summary>
public sealed record UnlinkDeviceCommand : IRequest<Unit>
{
    public string DeviceId { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
}

/// <summary>
/// Validator for UnlinkDeviceCommand.
/// </summary>
public sealed class UnlinkDeviceCommandValidator : AbstractValidator<UnlinkDeviceCommand>
{
    public UnlinkDeviceCommandValidator()
    {
        RuleFor(x => x.DeviceId)
            .NotEmpty().WithMessage("Device ID is required.");

        RuleFor(x => x.FamilyId)
            .NotEmpty().WithMessage("Family ID is required.");
    }
}

/// <summary>
/// Handler for UnlinkDeviceCommand.
/// </summary>
public sealed class UnlinkDeviceCommandHandler : IRequestHandler<UnlinkDeviceCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UnlinkDeviceCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Unit> Handle(UnlinkDeviceCommand request, CancellationToken cancellationToken)
    {
        var device = await _unitOfWork.Devices.GetByIdAsync(request.DeviceId, request.FamilyId, cancellationToken)
            ?? throw new NotFoundException("Device", request.DeviceId);

        if (!device.IsLinked)
        {
            throw new FluentValidation.ValidationException([new FluentValidation.Results.ValidationFailure(
                "DeviceId", "Device is not linked to any family.")]);
        }

        device.Unlink(_currentUserService.UserId ?? "system");

        await _unitOfWork.Devices.UpdateAsync(device, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
