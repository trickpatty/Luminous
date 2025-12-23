using FluentAssertions;
using Luminous.Application.Common.Exceptions;
using Luminous.Application.Common.Interfaces;
using Luminous.Application.Features.Devices.Commands;
using Luminous.Domain.Entities;
using Luminous.Domain.Enums;
using Luminous.Domain.Interfaces;
using Moq;

namespace Luminous.Application.Tests.Devices.Commands;

public class LinkDeviceCommandTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IDeviceRepository> _deviceRepositoryMock;
    private readonly Mock<IFamilyRepository> _familyRepositoryMock;
    private readonly LinkDeviceCommandHandler _handler;

    public LinkDeviceCommandTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _tokenServiceMock = new Mock<ITokenService>();
        _deviceRepositoryMock = new Mock<IDeviceRepository>();
        _familyRepositoryMock = new Mock<IFamilyRepository>();

        _unitOfWorkMock.Setup(x => x.Devices).Returns(_deviceRepositoryMock.Object);
        _unitOfWorkMock.Setup(x => x.Families).Returns(_familyRepositoryMock.Object);
        _currentUserServiceMock.Setup(x => x.UserId).Returns("user-123");

        _handler = new LinkDeviceCommandHandler(
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object,
            _tokenServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidLinkCode_ShouldLinkDeviceAndReturnToken()
    {
        // Arrange
        var device = Device.CreateWithLinkCode(DeviceType.Display);
        var family = new Family { Id = "family-123", Name = "Test Family" };

        var command = new LinkDeviceCommand
        {
            LinkCode = device.LinkCode!,
            FamilyId = family.Id,
            DeviceName = "Kitchen Display"
        };

        _deviceRepositoryMock.Setup(x => x.GetByLinkCodeAsync(device.LinkCode!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);
        _familyRepositoryMock.Setup(x => x.GetByIdAsync(family.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(family);
        _tokenServiceMock.Setup(x => x.GenerateDeviceToken(It.IsAny<Device>(), It.IsAny<string>()))
            .Returns(new DTOs.AuthResultDto
            {
                AccessToken = "test-token",
                RefreshToken = "refresh-token",
                TokenType = "Bearer",
                ExpiresIn = 2592000
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Device.Name.Should().Be("Kitchen Display");
        result.Device.FamilyId.Should().Be(family.Id);
        result.Device.IsLinked.Should().BeTrue();
        result.AccessToken.Should().Be("test-token");
        result.RefreshToken.Should().Be("refresh-token");

        _deviceRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Device>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidLinkCode_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new LinkDeviceCommand
        {
            LinkCode = "123456",
            FamilyId = "family-123",
            DeviceName = "Kitchen Display"
        };

        _deviceRepositoryMock.Setup(x => x.GetByLinkCodeAsync("123456", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Device?)null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*link code*");
    }

    [Fact]
    public async Task Handle_WithExpiredLinkCode_ShouldThrowValidationException()
    {
        // Arrange
        var device = new Device
        {
            Type = DeviceType.Display,
            LinkCode = "123456",
            LinkCodeExpiry = DateTime.UtcNow.AddMinutes(-5) // Expired
        };

        var command = new LinkDeviceCommand
        {
            LinkCode = "123456",
            FamilyId = "family-123",
            DeviceName = "Kitchen Display"
        };

        _deviceRepositoryMock.Setup(x => x.GetByLinkCodeAsync("123456", It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*expired*");
    }

    [Fact]
    public async Task Handle_WithInvalidFamily_ShouldThrowNotFoundException()
    {
        // Arrange
        var device = Device.CreateWithLinkCode(DeviceType.Display);

        var command = new LinkDeviceCommand
        {
            LinkCode = device.LinkCode!,
            FamilyId = "invalid-family",
            DeviceName = "Kitchen Display"
        };

        _deviceRepositoryMock.Setup(x => x.GetByLinkCodeAsync(device.LinkCode!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);
        _familyRepositoryMock.Setup(x => x.GetByIdAsync("invalid-family", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Family?)null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Family*");
    }
}

public class LinkDeviceCommandValidatorTests
{
    private readonly LinkDeviceCommandValidator _validator;

    public LinkDeviceCommandValidatorTests()
    {
        _validator = new LinkDeviceCommandValidator();
    }

    [Fact]
    public void Validate_WithValidData_ShouldPass()
    {
        // Arrange
        var command = new LinkDeviceCommand
        {
            LinkCode = "123456",
            FamilyId = "family-123",
            DeviceName = "Kitchen Display"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("12345")]
    [InlineData("1234567")]
    public void Validate_WithInvalidLinkCode_ShouldFail(string linkCode)
    {
        // Arrange
        var command = new LinkDeviceCommand
        {
            LinkCode = linkCode,
            FamilyId = "family-123",
            DeviceName = "Kitchen Display"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "LinkCode");
    }

    [Fact]
    public void Validate_WithEmptyFamilyId_ShouldFail()
    {
        // Arrange
        var command = new LinkDeviceCommand
        {
            LinkCode = "123456",
            FamilyId = "",
            DeviceName = "Kitchen Display"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FamilyId");
    }

    [Fact]
    public void Validate_WithDeviceNameTooLong_ShouldFail()
    {
        // Arrange
        var command = new LinkDeviceCommand
        {
            LinkCode = "123456",
            FamilyId = "family-123",
            DeviceName = new string('a', 51)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DeviceName");
    }
}
