using FluentAssertions;
using Luminous.Application.Common.Exceptions;
using Luminous.Application.Common.Interfaces;
using Luminous.Application.Features.Devices.Commands;
using Luminous.Domain.Entities;
using Luminous.Domain.Enums;
using Luminous.Domain.Interfaces;
using MediatR;
using Moq;
using NanoidDotNet;
using Xunit;

namespace Luminous.Application.Tests.Devices.Commands;

public class UnlinkDeviceCommandTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IDeviceRepository> _deviceRepositoryMock;
    private readonly UnlinkDeviceCommandHandler _handler;

    public UnlinkDeviceCommandTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _deviceRepositoryMock = new Mock<IDeviceRepository>();

        _unitOfWorkMock.Setup(x => x.Devices).Returns(_deviceRepositoryMock.Object);
        _currentUserServiceMock.Setup(x => x.UserId).Returns("user-123");

        _handler = new UnlinkDeviceCommandHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithLinkedDevice_ShouldDeleteDevice()
    {
        // Arrange
        var familyId = Nanoid.Generate();
        var device = Device.CreateWithLinkCode(DeviceType.Display);
        device.Link(familyId, "Kitchen Display", "user-123");

        var command = new UnlinkDeviceCommand
        {
            DeviceId = device.Id,
            FamilyId = familyId
        };

        _deviceRepositoryMock.Setup(x => x.GetByIdAsync(device.Id, familyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        _deviceRepositoryMock.Verify(x => x.DeleteAsync(device, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentDevice_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new UnlinkDeviceCommand
        {
            DeviceId = "non-existent",
            FamilyId = "family-123"
        };

        _deviceRepositoryMock.Setup(x => x.GetByIdAsync("non-existent", "family-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Device?)null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithUnlinkedDevice_ShouldThrowValidationException()
    {
        // Arrange
        var device = Device.CreateWithLinkCode(DeviceType.Display);

        var command = new UnlinkDeviceCommand
        {
            DeviceId = device.Id,
            FamilyId = "family-123"
        };

        _deviceRepositoryMock.Setup(x => x.GetByIdAsync(device.Id, "family-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<FluentValidation.ValidationException>()
            .WithMessage("*not linked*");
    }
}

public class UnlinkDeviceCommandValidatorTests
{
    private readonly UnlinkDeviceCommandValidator _validator;

    public UnlinkDeviceCommandValidatorTests()
    {
        _validator = new UnlinkDeviceCommandValidator();
    }

    [Fact]
    public void Validate_WithValidData_ShouldPass()
    {
        // Arrange
        var command = new UnlinkDeviceCommand
        {
            DeviceId = "device-123",
            FamilyId = "family-123"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyDeviceId_ShouldFail()
    {
        // Arrange
        var command = new UnlinkDeviceCommand
        {
            DeviceId = "",
            FamilyId = "family-123"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DeviceId");
    }
}
