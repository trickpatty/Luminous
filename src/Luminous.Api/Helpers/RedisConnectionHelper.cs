namespace Luminous.Api.Helpers;

/// <summary>
/// Helper class for parsing Redis connection strings.
/// Azure Redis Cache AVM exports connection strings in URL format (rediss://),
/// but StackExchange.Redis expects a different configuration format.
/// </summary>
public static class RedisConnectionHelper
{
    /// <summary>
    /// Converts a Redis URL format connection string to StackExchange.Redis format.
    /// </summary>
    /// <param name="connectionString">The connection string (URL or configuration format)</param>
    /// <returns>A StackExchange.Redis compatible configuration string</returns>
    /// <remarks>
    /// Supports the following input formats:
    /// - URL format: rediss://:password@hostname:port or redis://:password@hostname:port
    /// - Configuration format: hostname:port,password=xxx,ssl=True (passed through unchanged)
    /// </remarks>
    public static string ConvertToStackExchangeFormat(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            return connectionString;
        }

        // Check if it's already in StackExchange.Redis format (contains comma-separated options)
        if (!connectionString.StartsWith("redis://", StringComparison.OrdinalIgnoreCase) &&
            !connectionString.StartsWith("rediss://", StringComparison.OrdinalIgnoreCase))
        {
            return connectionString;
        }

        // Parse the URL format
        var isSsl = connectionString.StartsWith("rediss://", StringComparison.OrdinalIgnoreCase);

        // Try to parse as URI
        if (!Uri.TryCreate(connectionString, UriKind.Absolute, out var uri))
        {
            throw new ArgumentException($"Invalid Redis connection string format: {MaskConnectionString(connectionString)}");
        }

        var host = uri.Host;
        var port = uri.Port > 0 ? uri.Port : (isSsl ? 6380 : 6379);

        // Validate that we have a host
        if (string.IsNullOrEmpty(host))
        {
            throw new ArgumentException($"Invalid Redis connection string - missing hostname: {MaskConnectionString(connectionString)}");
        }

        // Extract password from UserInfo (format is ":password" or "username:password")
        var password = string.Empty;
        if (!string.IsNullOrEmpty(uri.UserInfo))
        {
            var userInfoParts = uri.UserInfo.Split(':', 2);
            password = userInfoParts.Length > 1 ? userInfoParts[1] : userInfoParts[0];
            // URL decode the password in case it contains special characters
            password = Uri.UnescapeDataString(password);
        }

        // Build the StackExchange.Redis configuration string
        var configParts = new List<string> { $"{host}:{port}" };

        if (!string.IsNullOrEmpty(password))
        {
            configParts.Add($"password={password}");
        }

        if (isSsl)
        {
            configParts.Add("ssl=True");
        }

        // Add sensible defaults for Azure Redis
        configParts.Add("abortConnect=False");
        configParts.Add("connectTimeout=10000");
        configParts.Add("syncTimeout=10000");

        return string.Join(",", configParts);
    }

    /// <summary>
    /// Masks the connection string for safe logging.
    /// </summary>
    private static string MaskConnectionString(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            return connectionString;
        }

        // Mask everything after the protocol
        var protocolEnd = connectionString.IndexOf("://", StringComparison.Ordinal);
        if (protocolEnd > 0)
        {
            return connectionString[..(protocolEnd + 3)] + "***";
        }

        return "***";
    }
}
