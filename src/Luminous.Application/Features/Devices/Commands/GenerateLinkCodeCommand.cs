using FluentValidation;
using Luminous.Application.DTOs;
using Luminous.Domain.Entities;
using Luminous.Domain.Enums;
using Luminous.Domain.Interfaces;
using MediatR;

namespace Luminous.Application.Features.Devices.Commands;

/// <summary>
/// Command to generate a device link code.
/// </summary>
public sealed record GenerateLinkCodeCommand : IRequest<DeviceLinkCodeDto>
{
    public DeviceType DeviceType { get; init; } = DeviceType.Display;
    public string? Platform { get; init; }
}

/// <summary>
/// Validator for GenerateLinkCodeCommand.
/// </summary>
public sealed class GenerateLinkCodeCommandValidator : AbstractValidator<GenerateLinkCodeCommand>
{
    public GenerateLinkCodeCommandValidator()
    {
        RuleFor(x => x.DeviceType)
            .IsInEnum().WithMessage("Invalid device type.");
    }
}

/// <summary>
/// Handler for GenerateLinkCodeCommand.
/// </summary>
public sealed class GenerateLinkCodeCommandHandler : IRequestHandler<GenerateLinkCodeCommand, DeviceLinkCodeDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GenerateLinkCodeCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DeviceLinkCodeDto> Handle(GenerateLinkCodeCommand request, CancellationToken cancellationToken)
    {
        var device = Device.CreateWithLinkCode(request.DeviceType, request.Platform);

        await _unitOfWork.Devices.AddAsync(device, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DeviceLinkCodeDto
        {
            DeviceId = device.Id,
            LinkCode = device.LinkCode!,
            ExpiresAt = device.LinkCodeExpiry!.Value
        };
    }
}
