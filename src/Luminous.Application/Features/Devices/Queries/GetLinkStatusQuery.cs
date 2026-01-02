using FluentValidation;
using Luminous.Application.Common.Exceptions;
using Luminous.Application.Common.Interfaces;
using Luminous.Application.DTOs;
using Luminous.Domain.Interfaces;
using MediatR;

namespace Luminous.Application.Features.Devices.Queries;

/// <summary>
/// Query to get device link status (used by display app polling).
/// This endpoint does not require family authentication since the device is not yet linked.
/// </summary>
public sealed record GetLinkStatusQuery : IRequest<DeviceLinkStatusDto>
{
    public string DeviceId { get; init; } = string.Empty;
}

/// <summary>
/// Validator for GetLinkStatusQuery.
/// </summary>
public sealed class GetLinkStatusQueryValidator : AbstractValidator<GetLinkStatusQuery>
{
    public GetLinkStatusQueryValidator()
    {
        RuleFor(x => x.DeviceId)
            .NotEmpty().WithMessage("Device ID is required.");
    }
}

/// <summary>
/// Handler for GetLinkStatusQuery.
/// </summary>
public sealed class GetLinkStatusQueryHandler : IRequestHandler<GetLinkStatusQuery, DeviceLinkStatusDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;

    public GetLinkStatusQueryHandler(IUnitOfWork unitOfWork, ITokenService tokenService)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
    }

    public async Task<DeviceLinkStatusDto> Handle(GetLinkStatusQuery request, CancellationToken cancellationToken)
    {
        // For unlinked devices, the partition key is the device ID
        // Try to get the device with device ID as partition key
        var device = await _unitOfWork.Devices.GetByIdAsync(request.DeviceId, request.DeviceId, cancellationToken);

        if (device == null)
        {
            throw new NotFoundException("Device", request.DeviceId);
        }

        // Determine status
        if (device.IsLinked)
        {
            // Device has been linked - generate a new token for the device
            var authResult = _tokenService.GenerateDeviceToken(device, device.FamilyId);
            var familyName = await GetFamilyNameAsync(device.FamilyId, cancellationToken);

            return new DeviceLinkStatusDto
            {
                Status = "linked",
                DeviceToken = authResult.AccessToken,
                FamilyId = device.FamilyId,
                FamilyName = familyName
            };
        }

        if (!device.IsLinkCodeValid)
        {
            // Link code has expired
            return new DeviceLinkStatusDto
            {
                Status = "expired"
            };
        }

        // Still pending
        return new DeviceLinkStatusDto
        {
            Status = "pending"
        };
    }

    private async Task<string?> GetFamilyNameAsync(string familyId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(familyId))
            return null;

        var family = await _unitOfWork.Families.GetByIdAsync(familyId, familyId, cancellationToken);
        return family?.Name;
    }
}
