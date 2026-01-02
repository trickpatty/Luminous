using FluentAssertions;
using Luminous.Domain.Entities;
using Luminous.Domain.Enums;
using NanoidDotNet;
using Xunit;

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
    public void CreateWithLinkCode_ShouldSetFamilyIdToDeviceId()
    {
        // Arrange & Act
        var device = Device.CreateWithLinkCode(DeviceType.Display);

        // Assert
        // Unlinked devices use their own ID as the partition key (FamilyId)
        // This is required for CosmosDB partition key consistency
        device.FamilyId.Should().Be(device.Id);
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

    [Fact]
    public void Unlink_WhenLinked_ShouldUnlinkDevice()
    {
        // Arrange
        var device = Device.CreateWithLinkCode(DeviceType.Display);
        device.Link(Nanoid.Generate(), "Kitchen Display", "user-id");

        // Act
        device.Unlink("admin-id");

        // Assert
        device.IsLinked.Should().BeFalse();
        device.FamilyId.Should().BeEmpty();
        device.Name.Should().BeEmpty();
        device.LinkedAt.Should().BeNull();
        device.LinkedBy.Should().BeNull();
        device.IsActive.Should().BeFalse();
        device.ModifiedBy.Should().Be("admin-id");
    }

    [Fact]
    public void Unlink_WhenNotLinked_ShouldThrowException()
    {
        // Arrange
        var device = Device.CreateWithLinkCode(DeviceType.Display);

        // Act
        var act = () => device.Unlink("user-id");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Device is not linked.");
    }

    [Fact]
    public void Rename_ShouldUpdateDeviceName()
    {
        // Arrange
        var device = Device.CreateWithLinkCode(DeviceType.Display);
        device.Link(Nanoid.Generate(), "Kitchen Display", "user-id");

        // Act
        device.Rename("Living Room Display", "admin-id");

        // Assert
        device.Name.Should().Be("Living Room Display");
        device.ModifiedBy.Should().Be("admin-id");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Rename_WithInvalidName_ShouldThrowException(string? name)
    {
        // Arrange
        var device = Device.CreateWithLinkCode(DeviceType.Display);
        device.Link(Nanoid.Generate(), "Kitchen Display", "user-id");

        // Act
        var act = () => device.Rename(name!, "admin-id");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Device name*");
    }

    [Fact]
    public void UpdateSettings_ShouldUpdateDeviceSettings()
    {
        // Arrange
        var device = Device.CreateWithLinkCode(DeviceType.Display);
        device.Link(Nanoid.Generate(), "Kitchen Display", "user-id");
        var newSettings = new Luminous.Domain.ValueObjects.DeviceSettings
        {
            Brightness = 75,
            AutoBrightness = false,
            DefaultView = "week",
            Orientation = "landscape",
            Volume = 30,
            SoundEnabled = false
        };

        // Act
        device.UpdateSettings(newSettings, "admin-id");

        // Assert
        device.Settings.Brightness.Should().Be(75);
        device.Settings.AutoBrightness.Should().BeFalse();
        device.Settings.DefaultView.Should().Be("week");
        device.Settings.Orientation.Should().Be("landscape");
        device.Settings.Volume.Should().Be(30);
        device.Settings.SoundEnabled.Should().BeFalse();
        device.ModifiedBy.Should().Be("admin-id");
    }

    [Fact]
    public void UpdateSettings_WithNullSettings_ShouldThrowException()
    {
        // Arrange
        var device = Device.CreateWithLinkCode(DeviceType.Display);

        // Act
        var act = () => device.UpdateSettings(null!, "user-id");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var device = Device.CreateWithLinkCode(DeviceType.Display);
        device.Link(Nanoid.Generate(), "Kitchen Display", "user-id");
        device.IsActive.Should().BeTrue();

        // Act
        device.Deactivate("admin-id");

        // Assert
        device.IsActive.Should().BeFalse();
        device.ModifiedBy.Should().Be("admin-id");
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var device = Device.CreateWithLinkCode(DeviceType.Display);
        device.Link(Nanoid.Generate(), "Kitchen Display", "user-id");
        device.Deactivate("admin-id");
        device.IsActive.Should().BeFalse();

        // Act
        device.Activate("admin-id");

        // Assert
        device.IsActive.Should().BeTrue();
        device.ModifiedBy.Should().Be("admin-id");
    }

    [Theory]
    [InlineData(DeviceType.Display)]
    [InlineData(DeviceType.Mobile)]
    [InlineData(DeviceType.Web)]
    public void CreateWithLinkCode_ShouldSetCorrectDeviceType(DeviceType deviceType)
    {
        // Arrange & Act
        var device = Device.CreateWithLinkCode(deviceType, "iOS 17.0");

        // Assert
        device.Type.Should().Be(deviceType);
        device.Platform.Should().Be("iOS 17.0");
    }
}
