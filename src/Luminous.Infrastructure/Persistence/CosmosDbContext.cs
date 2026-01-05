using Azure.Identity;
using Luminous.Infrastructure.Persistence.Configuration;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Luminous.Infrastructure.Persistence;

/// <summary>
/// Cosmos DB context for managing database and container access.
/// </summary>
public sealed class CosmosDbContext : IAsyncDisposable
{
    private readonly CosmosClient _client;
    private readonly CosmosDbSettings _settings;
    private readonly ILogger<CosmosDbContext> _logger;
    private Database? _database;
    private bool _disposed;

    public CosmosDbContext(IOptions<CosmosDbSettings> settings, ILogger<CosmosDbContext> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        var options = new CosmosClientOptions
        {
            // Use custom System.Text.Json serializer to properly handle [JsonInclude] on private setters
            Serializer = new SystemTextJsonCosmosSerializer(),
            ConnectionMode = ConnectionMode.Direct,
            MaxRetryAttemptsOnRateLimitedRequests = _settings.MaxRetryAttempts,
            MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(_settings.MaxRetryWaitTimeSeconds)
        };

        if (_settings.PreferredRegions.Count > 0)
        {
            options.ApplicationPreferredRegions = _settings.PreferredRegions;
        }

        if (_settings.UseManagedIdentity)
        {
            _client = new CosmosClient(_settings.AccountEndpoint, new DefaultAzureCredential(), options);
        }
        else if (!string.IsNullOrEmpty(_settings.AccountKey))
        {
            _client = new CosmosClient(_settings.AccountEndpoint, _settings.AccountKey, options);
        }
        else
        {
            throw new InvalidOperationException("Either AccountKey or UseManagedIdentity must be configured.");
        }

        _logger.LogInformation("CosmosDbContext initialized for database {DatabaseName}", _settings.DatabaseName);
    }

    /// <summary>
    /// Gets the Cosmos DB database.
    /// </summary>
    public Task<Database> GetDatabaseAsync(CancellationToken cancellationToken = default)
    {
        if (_database == null)
        {
            _database = _client.GetDatabase(_settings.DatabaseName);
            _logger.LogDebug("Connected to database {DatabaseName}", _settings.DatabaseName);
        }

        return Task.FromResult(_database);
    }

    /// <summary>
    /// Gets a container by name.
    /// </summary>
    public async Task<Container> GetContainerAsync(string containerName, CancellationToken cancellationToken = default)
    {
        var database = await GetDatabaseAsync(cancellationToken);
        return database.GetContainer(containerName);
    }

    /// <summary>
    /// Gets the families container.
    /// </summary>
    public Task<Container> GetFamiliesContainerAsync(CancellationToken cancellationToken = default)
        => GetContainerAsync(ContainerNames.Families, cancellationToken);

    /// <summary>
    /// Gets the users container.
    /// </summary>
    public Task<Container> GetUsersContainerAsync(CancellationToken cancellationToken = default)
        => GetContainerAsync(ContainerNames.Users, cancellationToken);

    /// <summary>
    /// Gets the events container.
    /// </summary>
    public Task<Container> GetEventsContainerAsync(CancellationToken cancellationToken = default)
        => GetContainerAsync(ContainerNames.Events, cancellationToken);

    /// <summary>
    /// Gets the chores container.
    /// </summary>
    public Task<Container> GetChoresContainerAsync(CancellationToken cancellationToken = default)
        => GetContainerAsync(ContainerNames.Chores, cancellationToken);

    /// <summary>
    /// Gets the devices container.
    /// </summary>
    public Task<Container> GetDevicesContainerAsync(CancellationToken cancellationToken = default)
        => GetContainerAsync(ContainerNames.Devices, cancellationToken);

    /// <summary>
    /// Gets the credentials container.
    /// </summary>
    public Task<Container> GetCredentialsContainerAsync(CancellationToken cancellationToken = default)
        => GetContainerAsync(ContainerNames.Credentials, cancellationToken);

    /// <summary>
    /// Gets the invitations container.
    /// </summary>
    public Task<Container> GetInvitationsContainerAsync(CancellationToken cancellationToken = default)
        => GetContainerAsync(ContainerNames.Invitations, cancellationToken);

    public ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            _client.Dispose();
            _disposed = true;
        }
        return ValueTask.CompletedTask;
    }
}
