using FluentAssertions;
using Luminous.Application.Common.Exceptions;
using Luminous.Application.Features.Devices.Commands;
using Luminous.Domain.Entities;
using Luminous.Domain.Enums;
using Luminous.Domain.Interfaces;
using Moq;
using NanoidDotNet;
using Xunit;

namespace Luminous.Application.Tests.Devices.Commands;

public class RecordHeartbeatCommandTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IDeviceRepository> _deviceRepositoryMock;
    private readonly RecordHeartbeatCommandHandler _handler;

    public RecordHeartbeatCommandTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _deviceRepositoryMock = new Mock<IDeviceRepository>();

        _unitOfWorkMock.Setup(x => x.Devices).Returns(_deviceRepositoryMock.Object);

        _handler = new RecordHeartbeatCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithLinkedDevice_ShouldRecordHeartbeat()
    {
        // Arrange
        var familyId = Nanoid.Generate();
        var device = Device.CreateWithLinkCode(DeviceType.Display);
        device.Link(familyId, "Kitchen Display", "user-123");
        var previousLastSeen = device.LastSeenAt;

        var command = new RecordHeartbeatCommand
        {
            DeviceId = device.Id,
            FamilyId = familyId,
            AppVersion = "2.0.0"
        };

        _deviceRepositoryMock.Setup(x => x.GetByIdAsync(device.Id, familyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.DeviceId.Should().Be(device.Id);
        result.AppVersion.Should().Be("2.0.0");
        result.LastSeenAt.Should().BeOnOrAfter(previousLastSeen);
        _deviceRepositoryMock.Verify(x => x.UpdateAsync(device, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentDevice_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new RecordHeartbeatCommand
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

        var command = new RecordHeartbeatCommand
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

public class RecordHeartbeatCommandValidatorTests
{
    private readonly RecordHeartbeatCommandValidator _validator;

    public RecordHeartbeatCommandValidatorTests()
    {
        _validator = new RecordHeartbeatCommandValidator();
    }

    [Fact]
    public void Validate_WithValidData_ShouldPass()
    {
        // Arrange
        var command = new RecordHeartbeatCommand
        {
            DeviceId = "device-123",
            FamilyId = "family-123",
            AppVersion = "1.0.0"
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
        var command = new RecordHeartbeatCommand
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

    [Fact]
    public void Validate_WithAppVersionTooLong_ShouldFail()
    {
        // Arrange
        var command = new RecordHeartbeatCommand
        {
            DeviceId = "device-123",
            FamilyId = "family-123",
            AppVersion = new string('a', 51)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "AppVersion");
    }
}
