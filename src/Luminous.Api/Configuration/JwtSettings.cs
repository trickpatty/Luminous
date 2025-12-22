namespace Luminous.Api.Configuration;

/// <summary>
/// JWT authentication settings for the API.
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "Jwt";

    /// <summary>
    /// Gets or sets the secret key used to sign JWT tokens (development only).
    /// In production, tokens are validated against the external identity provider.
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the JWT issuer.
    /// </summary>
    public string Issuer { get; set; } = "https://luminous.local";

    /// <summary>
    /// Gets or sets the JWT audience.
    /// </summary>
    public string Audience { get; set; } = "luminous-api";

    /// <summary>
    /// Gets or sets the token expiration time in minutes.
    /// </summary>
    public int ExpirationMinutes { get; set; } = 60;

    /// <summary>
    /// Gets or sets the refresh token expiration time in days.
    /// </summary>
    public int RefreshExpirationDays { get; set; } = 7;

    /// <summary>
    /// Gets or sets whether local JWT token generation is enabled.
    /// Should only be true in development environments.
    /// </summary>
    public bool EnableLocalTokenGeneration { get; set; } = false;
}
