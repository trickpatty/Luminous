namespace Luminous.Api.Configuration;

/// <summary>
/// SignalR configuration settings.
/// </summary>
public class SignalRSettings
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "SignalR";

    /// <summary>
    /// Gets or sets the Azure SignalR Service connection string.
    /// When null or empty, uses self-hosted SignalR.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets whether to use Azure SignalR Service in Serverless mode.
    /// Default mode is "Default" which handles connections server-side.
    /// </summary>
    public bool UseServerlessMode { get; set; } = false;

    /// <summary>
    /// Gets or sets the maximum number of items to keep in memory for debugging.
    /// Only applicable in development environments.
    /// </summary>
    public int DebugLogLimit { get; set; } = 100;

    /// <summary>
    /// Gets or sets the hub endpoint path.
    /// </summary>
    public string HubPath { get; set; } = "/hubs/sync";
}
