using System.Net;
using Luminous.Domain.Common;
using Luminous.Domain.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace Luminous.Infrastructure.Persistence.Repositories;

/// <summary>
/// Base class for Cosmos DB repositories.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public abstract class CosmosRepositoryBase<T> : IRepository<T> where T : Entity
{
    protected readonly CosmosDbContext Context;
    protected readonly ILogger Logger;
    protected readonly string ContainerName;

    protected CosmosRepositoryBase(CosmosDbContext context, ILogger logger, string containerName)
    {
        Context = context;
        Logger = logger;
        ContainerName = containerName;
    }

    /// <summary>
    /// Gets the partition key for an entity.
    /// </summary>
    protected abstract PartitionKey GetPartitionKey(T entity);

    /// <summary>
    /// Gets the partition key path for this container.
    /// </summary>
    protected abstract string GetPartitionKeyPath();

    /// <summary>
    /// Gets the container for this repository.
    /// </summary>
    protected async Task<Container> GetContainerAsync(CancellationToken cancellationToken = default)
    {
        return await Context.GetContainerAsync(ContainerName, cancellationToken);
    }

    public virtual async Task<T?> GetByIdAsync(string id, string? partitionKey = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var container = await GetContainerAsync(cancellationToken);
            var pk = partitionKey != null ? new PartitionKey(partitionKey) : PartitionKey.None;

            var response = await container.ReadItemAsync<T>(id, pk, cancellationToken: cancellationToken);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(string? partitionKey = null, CancellationToken cancellationToken = default)
    {
        var container = await GetContainerAsync(cancellationToken);
        var query = "SELECT * FROM c";

        var queryDefinition = new QueryDefinition(query);
        var queryOptions = partitionKey != null
            ? new QueryRequestOptions { PartitionKey = new PartitionKey(partitionKey) }
            : new QueryRequestOptions();

        var iterator = container.GetItemQueryIterator<T>(queryDefinition, requestOptions: queryOptions);
        var results = new List<T>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);
            results.AddRange(response);
        }

        return results;
    }

    public virtual async Task<bool> ExistsAsync(string id, string? partitionKey = null, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, partitionKey, cancellationToken);
        return entity != null;
    }

    public virtual async Task<int> CountAsync(string? partitionKey = null, CancellationToken cancellationToken = default)
    {
        var container = await GetContainerAsync(cancellationToken);
        var query = "SELECT VALUE COUNT(1) FROM c";

        var queryDefinition = new QueryDefinition(query);
        var queryOptions = partitionKey != null
            ? new QueryRequestOptions { PartitionKey = new PartitionKey(partitionKey) }
            : new QueryRequestOptions();

        var iterator = container.GetItemQueryIterator<int>(queryDefinition, requestOptions: queryOptions);
        var response = await iterator.ReadNextAsync(cancellationToken);

        return response.FirstOrDefault();
    }

    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        var container = await GetContainerAsync(cancellationToken);
        var response = await container.CreateItemAsync(entity, GetPartitionKey(entity), cancellationToken: cancellationToken);

        Logger.LogDebug("Created {EntityType} with ID {Id}", typeof(T).Name, entity.Id);
        return response.Resource;
    }

    public virtual async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        var container = await GetContainerAsync(cancellationToken);
        entity.ModifiedAt = DateTime.UtcNow;

        var response = await container.ReplaceItemAsync(entity, entity.Id, GetPartitionKey(entity), cancellationToken: cancellationToken);

        Logger.LogDebug("Updated {EntityType} with ID {Id}", typeof(T).Name, entity.Id);
        return response.Resource;
    }

    public virtual async Task<T> UpsertAsync(T entity, CancellationToken cancellationToken = default)
    {
        var container = await GetContainerAsync(cancellationToken);
        var response = await container.UpsertItemAsync(entity, GetPartitionKey(entity), cancellationToken: cancellationToken);

        Logger.LogDebug("Upserted {EntityType} with ID {Id}", typeof(T).Name, entity.Id);
        return response.Resource;
    }

    public virtual async Task DeleteAsync(string id, string? partitionKey = null, CancellationToken cancellationToken = default)
    {
        var container = await GetContainerAsync(cancellationToken);
        var pk = partitionKey != null ? new PartitionKey(partitionKey) : PartitionKey.None;

        await container.DeleteItemAsync<T>(id, pk, cancellationToken: cancellationToken);
        Logger.LogDebug("Deleted {EntityType} with ID {Id}", typeof(T).Name, id);
    }

    public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        var container = await GetContainerAsync(cancellationToken);
        await container.DeleteItemAsync<T>(entity.Id, GetPartitionKey(entity), cancellationToken: cancellationToken);

        Logger.LogDebug("Deleted {EntityType} with ID {Id}", typeof(T).Name, entity.Id);
    }

    /// <summary>
    /// Executes a query and returns the results.
    /// </summary>
    protected async Task<IReadOnlyList<T>> QueryAsync(
        QueryDefinition queryDefinition,
        string? partitionKey = null,
        CancellationToken cancellationToken = default)
    {
        var container = await GetContainerAsync(cancellationToken);
        var queryOptions = partitionKey != null
            ? new QueryRequestOptions { PartitionKey = new PartitionKey(partitionKey) }
            : new QueryRequestOptions();

        var iterator = container.GetItemQueryIterator<T>(queryDefinition, requestOptions: queryOptions);
        var results = new List<T>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken);
            results.AddRange(response);
        }

        return results;
    }
}
