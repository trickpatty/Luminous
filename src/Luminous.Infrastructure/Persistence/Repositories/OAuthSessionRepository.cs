using Luminous.Domain.Entities;
using Luminous.Domain.Interfaces;
using Luminous.Infrastructure.Persistence.Configuration;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace Luminous.Infrastructure.Persistence.Repositories;

/// <summary>
/// Cosmos DB repository for OAuthSession entities.
/// </summary>
public sealed class OAuthSessionRepository : CosmosRepositoryBase<OAuthSession>, IOAuthSessionRepository
{
    public OAuthSessionRepository(CosmosDbContext context, ILogger<OAuthSessionRepository> logger)
        : base(context, logger, ContainerNames.OAuthSessions)
    {
    }

    protected override PartitionKey GetPartitionKey(OAuthSession entity) => new(entity.FamilyId);

    protected override string GetPartitionKeyPath() => "/familyId";

    public async Task<OAuthSession?> GetByStateAsync(
        string state,
        CancellationToken cancellationToken = default)
    {
        // Cross-partition query since we don't know the family from the state
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.state = @state AND c.expiresAt > @now")
            .WithParameter("@state", state)
            .WithParameter("@now", DateTime.UtcNow);

        var results = await QueryAsync(query, cancellationToken: cancellationToken);
        return results.FirstOrDefault();
    }

    public async Task DeleteExpiredAsync(CancellationToken cancellationToken = default)
    {
        // Find and delete expired sessions
        var query = new QueryDefinition(
            "SELECT c.id, c.familyId FROM c WHERE c.expiresAt <= @now")
            .WithParameter("@now", DateTime.UtcNow);

        var results = await QueryAsync(query, cancellationToken: cancellationToken);

        foreach (var session in results)
        {
            try
            {
                await DeleteAsync(session.Id, session.FamilyId, cancellationToken);
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // Already deleted, ignore
            }
        }
    }
}
