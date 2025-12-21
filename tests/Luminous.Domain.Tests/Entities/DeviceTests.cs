using FluentAssertions;
using Luminous.Domain.Entities;
using Luminous.Domain.Enums;
using Nanoid;

namespace Luminous.Domain.Tests.Entities;

public class DeviceTests
{
    [Fact]
    public void CreateWithLinkCode_ShouldGenerateSixDigitCode()
    {
        // Arrange & Act
        var device = Device.CreateWithLinkCode(DeviceType.Display);

        // Assert
        device.LinkCode.Should().NotBeNullOrEmpty();
        device.LinkCode.Should().HaveLength(6);
        device.LinkCode.Should().MatchRegex(@"^\d{6}$");
        device.LinkCodeExpiry.Should().BeAfter(DateTime.UtcNow);
        device.IsLinked.Should().BeFalse();
        device.IsLinkCodeValid.Should().BeTrue();
    }

    [Fact]
    public void Link_ShouldLinkDeviceToFamily()
    {
        // Arrange
        var device = Device.CreateWithLinkCode(DeviceType.Display);
        var familyId = Nanoid.Generate();
        var deviceName = "Kitchen Display";
        var linkedBy = "user-id";

        // Act
        device.Link(familyId, deviceName, linkedBy);

        // Assert
        device.FamilyId.Should().Be(familyId);
        device.Name.Should().Be(deviceName);
        device.LinkedBy.Should().Be(linkedBy);
        device.LinkedAt.Should().NotBeNull();
        device.IsLinked.Should().BeTrue();
        device.LinkCode.Should().BeNull();
        device.LinkCodeExpiry.Should().BeNull();
    }

    [Fact]
    public void RecordHeartbeat_ShouldUpdateLastSeenAt()
    {
        // Arrange
        var device = Device.CreateWithLinkCode(DeviceType.Display);
        var beforeHeartbeat = DateTime.UtcNow;

        // Act
        device.RecordHeartbeat("1.0.0");

        // Assert
        device.LastSeenAt.Should().BeOnOrAfter(beforeHeartbeat);
        device.AppVersion.Should().Be("1.0.0");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Link_WithInvalidFamilyId_ShouldThrowException(string? familyId)
    {
        // Arrange
        var device = Device.CreateWithLinkCode(DeviceType.Display);

        // Act
        var act = () => device.Link(familyId!, "Device Name", "user-id");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Family ID*");
    }
}
