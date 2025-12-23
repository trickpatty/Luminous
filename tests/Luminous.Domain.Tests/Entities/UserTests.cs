using FluentAssertions;
using Luminous.Domain.Entities;
using Luminous.Domain.Enums;
using NanoidDotNet;

namespace Luminous.Domain.Tests.Entities;

public class UserTests
{
    [Fact]
    public void CreateOwner_ShouldCreateUserWithOwnerRole()
    {
        // Arrange
        var familyId = Nanoid.Generate();
        var email = "owner@example.com";
        var displayName = "Family Owner";

        // Act
        var user = User.CreateOwner(familyId, email, displayName);

        // Assert
        user.FamilyId.Should().Be(familyId);
        user.Email.Should().Be(email);
        user.DisplayName.Should().Be(displayName);
        user.Role.Should().Be(UserRole.Owner);
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithRole_ShouldCreateUserWithSpecifiedRole()
    {
        // Arrange
        var familyId = Nanoid.Generate();
        var email = "child@example.com";
        var displayName = "Child User";

        // Act
        var user = User.Create(familyId, email, displayName, UserRole.Child);

        // Assert
        user.Role.Should().Be(UserRole.Child);
    }

    [Fact]
    public void RecordLogin_ShouldUpdateLastLoginAt()
    {
        // Arrange
        var user = User.CreateOwner("family-id", "user@example.com", "Test User");
        var beforeLogin = DateTime.UtcNow;

        // Act
        user.RecordLogin();

        // Assert
        user.LastLoginAt.Should().NotBeNull();
        user.LastLoginAt.Should().BeOnOrAfter(beforeLogin);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithInvalidFamilyId_ShouldThrowException(string? familyId)
    {
        // Arrange & Act
        var act = () => User.Create(familyId!, "email@test.com", "Name", UserRole.Adult);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Family ID*");
    }
}
