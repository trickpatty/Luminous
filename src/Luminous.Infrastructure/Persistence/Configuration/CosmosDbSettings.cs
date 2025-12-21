namespace Luminous.Infrastructure.Persistence.Configuration;

/// <summary>
/// Configuration settings for Azure Cosmos DB.
/// </summary>
public sealed class CosmosDbSettings
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "CosmosDb";

    /// <summary>
    /// Gets or sets the Cosmos DB account endpoint.
    /// </summary>
    public string AccountEndpoint { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Cosmos DB account key (optional if using managed identity).
    /// </summary>
    public string? AccountKey { get; set; }

    /// <summary>
    /// Gets or sets the database name.
    /// </summary>
    public string DatabaseName { get; set; } = "luminous";

    /// <summary>
    /// Gets or sets whether to use managed identity for authentication.
    /// </summary>
    public bool UseManagedIdentity { get; set; } = true;

    /// <summary>
    /// Gets or sets the preferred regions for geo-replication.
    /// </summary>
    public List<string> PreferredRegions { get; set; } = [];

    /// <summary>
    /// Gets or sets whether to enable connection pooling.
    /// </summary>
    public bool EnableConnectionPooling { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum retry attempts.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets the maximum retry wait time in seconds.
    /// </summary>
    public int MaxRetryWaitTimeSeconds { get; set; } = 30;
}
