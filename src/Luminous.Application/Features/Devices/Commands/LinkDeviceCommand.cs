using FluentValidation;
using Luminous.Application.Common.Exceptions;
using Luminous.Application.Common.Interfaces;
using Luminous.Application.DTOs;
using Luminous.Domain.Interfaces;
using MediatR;

namespace Luminous.Application.Features.Devices.Commands;

/// <summary>
/// Command to link a device to a family.
/// </summary>
public sealed record LinkDeviceCommand : IRequest<LinkedDeviceDto>
{
    public string LinkCode { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string DeviceName { get; init; } = string.Empty;
}

/// <summary>
/// Validator for LinkDeviceCommand.
/// </summary>
public sealed class LinkDeviceCommandValidator : AbstractValidator<LinkDeviceCommand>
{
    public LinkDeviceCommandValidator()
    {
        RuleFor(x => x.LinkCode)
            .NotEmpty().WithMessage("Link code is required.")
            .Length(6).WithMessage("Link code must be 6 digits.");

        RuleFor(x => x.FamilyId)
            .NotEmpty().WithMessage("Family ID is required.");

        RuleFor(x => x.DeviceName)
            .NotEmpty().WithMessage("Device name is required.")
            .MaximumLength(50).WithMessage("Device name must not exceed 50 characters.");
    }
}

/// <summary>
/// Handler for LinkDeviceCommand.
/// </summary>
public sealed class LinkDeviceCommandHandler : IRequestHandler<LinkDeviceCommand, LinkedDeviceDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITokenService _tokenService;

    public LinkDeviceCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ITokenService tokenService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _tokenService = tokenService;
    }

    public async Task<LinkedDeviceDto> Handle(LinkDeviceCommand request, CancellationToken cancellationToken)
    {
        // Find device by link code
        var device = await _unitOfWork.Devices.GetByLinkCodeAsync(request.LinkCode, cancellationToken)
            ?? throw new NotFoundException("Device with link code", request.LinkCode);

        // Verify link code is still valid
        if (!device.IsLinkCodeValid)
        {
            throw new FluentValidation.ValidationException([new FluentValidation.Results.ValidationFailure(
                "LinkCode", "Link code has expired. Please generate a new code.")]);
        }

        // Verify family exists
        var family = await _unitOfWork.Families.GetByIdAsync(request.FamilyId, cancellationToken)
            ?? throw new NotFoundException("Family", request.FamilyId);

        // Store the old partition key (device ID for unlinked devices)
        var oldPartitionKey = device.FamilyId;

        // Link the device (this changes FamilyId to the actual family ID)
        device.Link(
            request.FamilyId,
            request.DeviceName,
            _currentUserService.UserId ?? "system");

        // Move device to new partition (CosmosDB doesn't allow changing partition keys in place)
        await _unitOfWork.Devices.MoveToFamilyAsync(device, oldPartitionKey, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Generate device token
        var authResult = _tokenService.GenerateDeviceToken(device, request.FamilyId);

        var deviceDto = new DeviceDto
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

        return new LinkedDeviceDto
        {
            Device = deviceDto,
            AccessToken = authResult.AccessToken,
            RefreshToken = authResult.RefreshToken,
            TokenType = authResult.TokenType,
            ExpiresIn = authResult.ExpiresIn
        };
    }
}
