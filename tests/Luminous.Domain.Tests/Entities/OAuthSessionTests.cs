using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Luminous.Domain.Entities;
using Luminous.Domain.Enums;
using Xunit;

namespace Luminous.Domain.Tests.Entities;

public class OAuthSessionTests
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    [Fact]
    public void Create_ShouldSetExpiresAtTo15MinutesFromNow()
    {
        // Arrange
        var before = DateTime.UtcNow.AddMinutes(14);
        var after = DateTime.UtcNow.AddMinutes(16);

        // Act
        var session = OAuthSession.Create("family-id", CalendarProvider.Google, "https://example.com/callback", "user-id");

        // Assert
        session.ExpiresAt.Should().BeAfter(before);
        session.ExpiresAt.Should().BeBefore(after);
        session.IsExpired.Should().BeFalse();
        session.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public void Create_ShouldGenerateCryptographicallySecureState()
    {
        // Act
        var session1 = OAuthSession.Create("family-id", CalendarProvider.Google, "https://example.com/callback", "user-id");
        var session2 = OAuthSession.Create("family-id", CalendarProvider.Google, "https://example.com/callback", "user-id");

        // Assert
        session1.State.Should().NotBeNullOrEmpty();
        session2.State.Should().NotBeNullOrEmpty();
        session1.State.Should().NotBe(session2.State);
        session1.State.Should().HaveLength(43); // Base64 URL-safe encoding of 32 bytes
    }

    [Fact]
    public void Serialization_ShouldPreserveExpiresAtWithPrivateSetter()
    {
        // Arrange - This test ensures [JsonInclude] is working on private setters
        var session = OAuthSession.Create("family-id", CalendarProvider.Google, "https://example.com/callback", "user-id");
        var originalExpiresAt = session.ExpiresAt;

        // Act - Serialize and deserialize (simulating what CosmosDB does)
        var json = JsonSerializer.Serialize(session, SerializerOptions);
        var deserialized = JsonSerializer.Deserialize<OAuthSession>(json, SerializerOptions);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.ExpiresAt.Should().BeCloseTo(originalExpiresAt, TimeSpan.FromSeconds(1));
        deserialized.ExpiresAt.Should().NotBe(DateTime.MinValue, "ExpiresAt should not default to DateTime.MinValue after deserialization");
        deserialized.FamilyId.Should().Be("family-id");
        deserialized.Provider.Should().Be(CalendarProvider.Google);
        deserialized.State.Should().Be(session.State);
        deserialized.RedirectUri.Should().Be("https://example.com/callback");
        deserialized.IsCompleted.Should().BeFalse();
        deserialized.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void Serialization_ShouldProduceCamelCasePropertyNames()
    {
        // Arrange
        var session = OAuthSession.Create("family-id", CalendarProvider.Google, "https://example.com/callback", "user-id");

        // Act
        var json = JsonSerializer.Serialize(session, SerializerOptions);

        // Assert - Verify camelCase property names (matching CosmosDB storage format)
        json.Should().Contain("\"familyId\":");
        json.Should().Contain("\"expiresAt\":");
        json.Should().Contain("\"isCompleted\":");
        json.Should().Contain("\"redirectUri\":");
        json.Should().Contain("\"provider\":");
        json.Should().Contain("\"state\":");
    }

    [Fact]
    public void IsExpired_WhenExpiresAtInPast_ShouldReturnTrue()
    {
        // Arrange - Create session and serialize/deserialize with past ExpiresAt
        var json = """
            {
                "id": "test-id",
                "familyId": "family-id",
                "provider": "google",
                "state": "test-state",
                "redirectUri": "https://example.com/callback",
                "expiresAt": "2020-01-01T00:00:00Z",
                "isCompleted": false,
                "createdAt": "2020-01-01T00:00:00Z"
            }
            """;

        // Act
        var session = JsonSerializer.Deserialize<OAuthSession>(json, SerializerOptions);

        // Assert
        session.Should().NotBeNull();
        session!.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void Deserialization_WithMinValueExpiresAt_ShouldBeDetectable()
    {
        // Arrange - Simulate corrupted data with DateTime.MinValue
        var json = """
            {
                "id": "test-id",
                "familyId": "family-id",
                "provider": "google",
                "state": "test-state",
                "redirectUri": "https://example.com/callback",
                "expiresAt": "0001-01-01T00:00:00Z",
                "isCompleted": false,
                "createdAt": "2025-01-01T00:00:00Z"
            }
            """;

        // Act
        var session = JsonSerializer.Deserialize<OAuthSession>(json, SerializerOptions);

        // Assert - The system should detect this as corrupted data
        session.Should().NotBeNull();
        session!.ExpiresAt.Should().Be(DateTime.MinValue);
        session.IsExpired.Should().BeTrue("DateTime.MinValue is in the past");
    }
}
