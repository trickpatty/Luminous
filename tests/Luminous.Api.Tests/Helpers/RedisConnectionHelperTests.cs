using FluentAssertions;
using Luminous.Api.Helpers;
using Xunit;

namespace Luminous.Api.Tests.Helpers;

public class RedisConnectionHelperTests
{
    [Fact]
    public void ConvertToStackExchangeFormat_WithSslUrl_ShouldConvertCorrectly()
    {
        // Arrange
        var urlFormat = "rediss://:mypassword@myredis.redis.cache.windows.net:6380";

        // Act
        var result = RedisConnectionHelper.ConvertToStackExchangeFormat(urlFormat);

        // Assert
        result.Should().Contain("myredis.redis.cache.windows.net:6380");
        result.Should().Contain("password=mypassword");
        result.Should().Contain("ssl=True");
        result.Should().Contain("abortConnect=False");
    }

    [Fact]
    public void ConvertToStackExchangeFormat_WithNonSslUrl_ShouldConvertWithoutSsl()
    {
        // Arrange
        var urlFormat = "redis://:mypassword@localhost:6379";

        // Act
        var result = RedisConnectionHelper.ConvertToStackExchangeFormat(urlFormat);

        // Assert
        result.Should().Contain("localhost:6379");
        result.Should().Contain("password=mypassword");
        result.Should().NotContain("ssl=True");
        result.Should().Contain("abortConnect=False");
    }

    [Fact]
    public void ConvertToStackExchangeFormat_WithStackExchangeFormat_ShouldPassThrough()
    {
        // Arrange
        var stackExchangeFormat = "myredis.redis.cache.windows.net:6380,password=mypassword,ssl=True";

        // Act
        var result = RedisConnectionHelper.ConvertToStackExchangeFormat(stackExchangeFormat);

        // Assert
        result.Should().Be(stackExchangeFormat);
    }

    [Fact]
    public void ConvertToStackExchangeFormat_WithDefaultPort_ShouldUseCorrectDefault()
    {
        // Arrange - URL without explicit port
        var urlFormat = "rediss://:mypassword@myredis.redis.cache.windows.net";

        // Act
        var result = RedisConnectionHelper.ConvertToStackExchangeFormat(urlFormat);

        // Assert
        result.Should().Contain("myredis.redis.cache.windows.net:6380"); // SSL default port
        result.Should().Contain("ssl=True");
    }

    [Fact]
    public void ConvertToStackExchangeFormat_WithUrlEncodedPassword_ShouldDecodePassword()
    {
        // Arrange - password with special characters that need URL encoding
        var urlFormat = "rediss://:my%2Fpassword%3Dvalue@myredis.redis.cache.windows.net:6380";

        // Act
        var result = RedisConnectionHelper.ConvertToStackExchangeFormat(urlFormat);

        // Assert
        result.Should().Contain("password=my/password=value");
    }

    [Fact]
    public void ConvertToStackExchangeFormat_WithNullOrEmpty_ShouldReturnSameValue()
    {
        // Act & Assert
        RedisConnectionHelper.ConvertToStackExchangeFormat(null!).Should().BeNull();
        RedisConnectionHelper.ConvertToStackExchangeFormat("").Should().Be("");
    }

    [Fact]
    public void ConvertToStackExchangeFormat_WithMalformedUrl_ShouldThrowArgumentException()
    {
        // Arrange - a URL with empty host (colon before port but no hostname)
        var invalidUrl = "rediss://:password@:6380";

        // Act
        var act = () => RedisConnectionHelper.ConvertToStackExchangeFormat(invalidUrl);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("rediss://:pass@host:6380", "host:6380")]
    [InlineData("redis://:pass@localhost:6379", "localhost:6379")]
    [InlineData("rediss://:pass@my-redis.redis.cache.windows.net:6380", "my-redis.redis.cache.windows.net:6380")]
    public void ConvertToStackExchangeFormat_VariousUrls_ShouldExtractHostCorrectly(string url, string expectedHost)
    {
        // Act
        var result = RedisConnectionHelper.ConvertToStackExchangeFormat(url);

        // Assert
        result.Should().StartWith(expectedHost);
    }
}
