using FluentAssertions;
using Luminous.Application.Common.Exceptions;
using Luminous.Application.Common.Interfaces;
using Luminous.Application.Features.Families.Commands;
using Luminous.Domain.Entities;
using Luminous.Domain.Interfaces;
using Moq;
using Xunit;

namespace Luminous.Application.Tests.Families.Commands;

public class CreateFamilyCommandTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IDateTimeService> _dateTimeServiceMock;
    private readonly CreateFamilyCommandHandler _handler;

    public CreateFamilyCommandTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _dateTimeServiceMock = new Mock<IDateTimeService>();
        _dateTimeServiceMock.Setup(x => x.UtcNow).Returns(DateTime.UtcNow);

        _handler = new CreateFamilyCommandHandler(
            _unitOfWorkMock.Object,
            _dateTimeServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldCreateFamilyAndOwner()
    {
        // Arrange
        var command = new CreateFamilyCommand
        {
            Name = "Test Family",
            Timezone = "America/New_York",
            OwnerEmail = "owner@example.com",
            OwnerDisplayName = "Test Owner"
        };

        _unitOfWorkMock.Setup(x => x.Users.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _unitOfWorkMock.Setup(x => x.Families.AddAsync(It.IsAny<Family>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Family f, CancellationToken _) => f);
        _unitOfWorkMock.Setup(x => x.Users.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User u, CancellationToken _) => u);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(command.Name);
        result.Timezone.Should().Be(command.Timezone);
        result.MemberCount.Should().Be(1);

        _unitOfWorkMock.Verify(x => x.Families.AddAsync(It.IsAny<Family>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.Users.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithExistingEmail_ShouldThrowConflictException()
    {
        // Arrange
        var command = new CreateFamilyCommand
        {
            Name = "Test Family",
            Timezone = "UTC",
            OwnerEmail = "existing@example.com",
            OwnerDisplayName = "Test Owner"
        };

        _unitOfWorkMock.Setup(x => x.Users.EmailExistsAsync(command.OwnerEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*already in use*");
    }
}

public class CreateFamilyCommandValidatorTests
{
    private readonly CreateFamilyCommandValidator _validator;

    public CreateFamilyCommandValidatorTests()
    {
        _validator = new CreateFamilyCommandValidator();
    }

    [Fact]
    public void Validate_WithValidData_ShouldPass()
    {
        // Arrange
        var command = new CreateFamilyCommand
        {
            Name = "Test Family",
            Timezone = "America/New_York",
            OwnerEmail = "owner@example.com",
            OwnerDisplayName = "Test Owner"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyName_ShouldFail()
    {
        // Arrange
        var command = new CreateFamilyCommand
        {
            Name = "",
            Timezone = "UTC",
            OwnerEmail = "owner@example.com",
            OwnerDisplayName = "Test Owner"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_WithInvalidEmail_ShouldFail()
    {
        // Arrange
        var command = new CreateFamilyCommand
        {
            Name = "Test Family",
            Timezone = "UTC",
            OwnerEmail = "invalid-email",
            OwnerDisplayName = "Test Owner"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "OwnerEmail");
    }
}
