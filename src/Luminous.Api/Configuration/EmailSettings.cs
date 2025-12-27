namespace Luminous.Api.Configuration;

/// <summary>
/// Configuration settings for email service.
/// </summary>
public sealed class EmailSettings
{
    /// <summary>
    /// Configuration section name in appsettings.
    /// </summary>
    public const string SectionName = "Email";

    /// <summary>
    /// Gets or sets the Azure Communication Services connection string.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the sender email address.
    /// </summary>
    public string SenderAddress { get; set; } = "noreply@luminous.app";

    /// <summary>
    /// Gets or sets the sender display name.
    /// </summary>
    public string SenderName { get; set; } = "Luminous";

    /// <summary>
    /// Gets or sets the base URL for email links.
    /// </summary>
    public string BaseUrl { get; set; } = "https://luminous.app";

    /// <summary>
    /// Gets or sets the help center URL.
    /// </summary>
    public string HelpUrl { get; set; } = "https://luminous.app/help";

    /// <summary>
    /// Gets or sets whether to use development mode (logs emails instead of sending).
    /// </summary>
    public bool UseDevelopmentMode { get; set; }
}
