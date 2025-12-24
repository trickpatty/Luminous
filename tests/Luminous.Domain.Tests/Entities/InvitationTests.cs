using FluentAssertions;
using Luminous.Domain.Entities;
using Luminous.Domain.Enums;
using NanoidDotNet;
using Xunit;

namespace Luminous.Domain.Tests.Entities;

public class InvitationTests
{
    [Fact]
    public void Create_ShouldCreateInvitationWithPendingStatus()
    {
        // Arrange
        var familyId = Nanoid.Generate();
        var email = "invitee@example.com";
        var role = UserRole.Adult;
        var invitedBy = Nanoid.Generate();

        // Act
        var invitation = Invitation.Create(familyId, email, role, invitedBy);

        // Assert
        invitation.FamilyId.Should().Be(familyId);
        invitation.Email.Should().Be(email.ToLowerInvariant());
        invitation.Role.Should().Be(role);
        invitation.Status.Should().Be(InvitationStatus.Pending);
        invitation.CreatedBy.Should().Be(invitedBy);
        invitation.Code.Should().NotBeNullOrEmpty();
        invitation.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        invitation.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Create_WithMessage_ShouldIncludeMessage()
    {
        // Arrange
        var familyId = Nanoid.Generate();
        var message = "Welcome to the family!";

        // Act
        var invitation = Invitation.Create(familyId, "test@example.com", UserRole.Adult, "inviter", message: message);

        // Assert
        invitation.Message.Should().Be(message);
    }

    [Fact]
    public void Create_WithCustomExpiration_ShouldSetCorrectExpiry()
    {
        // Arrange
        var familyId = Nanoid.Generate();
        var expirationDays = 14;

        // Act
        var invitation = Invitation.Create(familyId, "test@example.com", UserRole.Adult, "inviter", expirationDays);

        // Assert
        var expectedMinExpiry = DateTime.UtcNow.AddDays(expirationDays - 1);
        var expectedMaxExpiry = DateTime.UtcNow.AddDays(expirationDays + 1);
        invitation.ExpiresAt.Should().BeAfter(expectedMinExpiry);
        invitation.ExpiresAt.Should().BeBefore(expectedMaxExpiry);
    }

    [Fact]
    public void Create_ShouldNormalizeEmail()
    {
        // Arrange
        var email = "  TEST@EXAMPLE.COM  ";

        // Act
        var invitation = Invitation.Create("family-id", email, UserRole.Adult, "inviter");

        // Assert
        invitation.Email.Should().Be("test@example.com");
    }

    [Fact]
    public void Accept_ShouldMarkInvitationAsAccepted()
    {
        // Arrange
        var invitation = Invitation.Create("family-id", "test@example.com", UserRole.Adult, "inviter");
        var userId = Nanoid.Generate();
        var beforeAccept = DateTime.UtcNow;

        // Act
        invitation.Accept(userId);

        // Assert
        invitation.Status.Should().Be(InvitationStatus.Accepted);
        invitation.AcceptedUserId.Should().Be(userId);
        invitation.AcceptedAt.Should().NotBeNull();
        invitation.AcceptedAt.Should().BeOnOrAfter(beforeAccept);
        invitation.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Accept_WhenAlreadyAccepted_ShouldThrowException()
    {
        // Arrange
        var invitation = Invitation.Create("family-id", "test@example.com", UserRole.Adult, "inviter");
        invitation.Accept("user-1");

        // Act
        var act = () => invitation.Accept("user-2");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*no longer valid*");
    }

    [Fact]
    public void Decline_ShouldMarkInvitationAsDeclined()
    {
        // Arrange
        var invitation = Invitation.Create("family-id", "test@example.com", UserRole.Adult, "inviter");

        // Act
        invitation.Decline();

        // Assert
        invitation.Status.Should().Be(InvitationStatus.Declined);
        invitation.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Decline_WhenNotPending_ShouldThrowException()
    {
        // Arrange
        var invitation = Invitation.Create("family-id", "test@example.com", UserRole.Adult, "inviter");
        invitation.Decline();

        // Act
        var act = () => invitation.Decline();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*pending*");
    }

    [Fact]
    public void Revoke_ShouldMarkInvitationAsRevoked()
    {
        // Arrange
        var invitation = Invitation.Create("family-id", "test@example.com", UserRole.Adult, "inviter");
        var revokedBy = Nanoid.Generate();

        // Act
        invitation.Revoke(revokedBy);

        // Assert
        invitation.Status.Should().Be(InvitationStatus.Revoked);
        invitation.ModifiedBy.Should().Be(revokedBy);
        invitation.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Revoke_WhenAccepted_ShouldThrowException()
    {
        // Arrange
        var invitation = Invitation.Create("family-id", "test@example.com", UserRole.Adult, "inviter");
        invitation.Accept("user-id");

        // Act
        var act = () => invitation.Revoke("revoker");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*pending*");
    }

    [Fact]
    public void IsValid_WhenExpired_ShouldReturnFalse()
    {
        // Arrange - create invitation with very short expiration
        var invitation = Invitation.Create("family-id", "test@example.com", UserRole.Adult, "inviter", expirationDays: 0);
        // Manually set to past date
        invitation.ExpiresAt = DateTime.UtcNow.AddMinutes(-1);

        // Assert
        invitation.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithInvalidFamilyId_ShouldThrowException(string? familyId)
    {
        // Act
        var act = () => Invitation.Create(familyId!, "email@test.com", UserRole.Adult, "inviter");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Family ID*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithInvalidEmail_ShouldThrowException(string? email)
    {
        // Act
        var act = () => Invitation.Create("family-id", email!, UserRole.Adult, "inviter");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Email*");
    }

    [Theory]
    [InlineData(UserRole.Owner)]
    [InlineData(UserRole.Admin)]
    [InlineData(UserRole.Adult)]
    [InlineData(UserRole.Teen)]
    [InlineData(UserRole.Child)]
    [InlineData(UserRole.Caregiver)]
    public void Create_WithDifferentRoles_ShouldSetCorrectRole(UserRole role)
    {
        // Act
        var invitation = Invitation.Create("family-id", "test@example.com", role, "inviter");

        // Assert
        invitation.Role.Should().Be(role);
    }
}
