using FluentAssertions;
using Luminous.Domain.Entities;
using Luminous.Domain.ValueObjects;

namespace Luminous.Domain.Tests.Entities;

public class FamilyTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateFamily()
    {
        // Arrange
        var name = "Test Family";
        var timezone = "America/New_York";
        var createdBy = "user@example.com";

        // Act
        var family = Family.Create(name, timezone, createdBy);

        // Assert
        family.Name.Should().Be(name);
        family.Timezone.Should().Be(timezone);
        family.CreatedBy.Should().Be(createdBy);
        family.Id.Should().NotBeNullOrEmpty();
        family.Settings.Should().NotBeNull();
        family.Subscription.Should().NotBeNull();
        family.Subscription!.Tier.Should().Be("free");
        family.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithInvalidName_ShouldThrowException(string? name)
    {
        // Arrange & Act
        var act = () => Family.Create(name!, "UTC", "user@example.com");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Family name*");
    }

    [Fact]
    public void UpdateSettings_ShouldUpdateSettingsAndModifiedAt()
    {
        // Arrange
        var family = Family.Create("Test Family", "UTC", "user@example.com");
        var newSettings = new FamilySettings
        {
            DefaultView = "week",
            PrivacyModeEnabled = false,
            ShowWeather = false
        };

        // Act
        family.UpdateSettings(newSettings, "modifier@example.com");

        // Assert
        family.Settings.DefaultView.Should().Be("week");
        family.Settings.PrivacyModeEnabled.Should().BeFalse();
        family.Settings.ShowWeather.Should().BeFalse();
        family.ModifiedAt.Should().NotBeNull();
        family.ModifiedBy.Should().Be("modifier@example.com");
    }
}
