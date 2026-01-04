using Luminous.Infrastructure.Persistence.Configuration;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Luminous.Infrastructure.Persistence;

/// <summary>
/// Hosted service that ensures all required CosmosDB containers exist at startup.
/// This provides resilience against infrastructure deployment gaps.
/// </summary>
public sealed class CosmosDbInitializer : IHostedService
{
    private readonly CosmosDbContext _context;
    private readonly CosmosDbSettings _settings;
    private readonly ILogger<CosmosDbInitializer> _logger;

    /// <summary>
    /// Container definitions with their partition key paths.
    /// Must match the Bicep infrastructure definition in infra/bicep/main.bicep.
    /// </summary>
    private static readonly (string Name, string PartitionKeyPath)[] RequiredContainers =
    [
        (ContainerNames.Families, "/id"),
        (ContainerNames.Users, "/familyId"),
        (ContainerNames.Events, "/familyId"),
        (ContainerNames.Chores, "/familyId"),
        (ContainerNames.Devices, "/familyId"),
        (ContainerNames.Routines, "/familyId"),
        (ContainerNames.Lists, "/familyId"),
        (ContainerNames.Meals, "/familyId"),
        (ContainerNames.Completions, "/familyId"),
        (ContainerNames.Invitations, "/familyId"),
        (ContainerNames.Credentials, "/userId"),
        (ContainerNames.OtpTokens, "/email"),
        (ContainerNames.RefreshTokens, "/userId"),
        (ContainerNames.CalendarConnections, "/familyId"),
        (ContainerNames.OAuthSessions, "/familyId")
    ];

    public CosmosDbInitializer(
        CosmosDbContext context,
        IOptions<CosmosDbSettings> settings,
        ILogger<CosmosDbInitializer> logger)
    {
        _context = context;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Initializing CosmosDB containers...");

        try
        {
            var database = await _context.GetDatabaseAsync(cancellationToken);

            foreach (var (name, partitionKeyPath) in RequiredContainers)
            {
                await EnsureContainerExistsAsync(database, name, partitionKeyPath, cancellationToken);
            }

            _logger.LogInformation("CosmosDB container initialization completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize CosmosDB containers");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task EnsureContainerExistsAsync(
        Database database,
        string containerName,
        string partitionKeyPath,
        CancellationToken cancellationToken)
    {
        try
        {
            var containerProperties = new ContainerProperties(containerName, partitionKeyPath);

            var response = await database.CreateContainerIfNotExistsAsync(
                containerProperties,
                throughput: null, // Use database-level throughput (serverless or autoscale)
                cancellationToken: cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.Created)
            {
                _logger.LogInformation("Created container {ContainerName} with partition key {PartitionKeyPath}",
                    containerName, partitionKeyPath);
            }
            else
            {
                _logger.LogDebug("Container {ContainerName} already exists", containerName);
            }
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Failed to ensure container {ContainerName} exists", containerName);
            throw;
        }
    }
}
