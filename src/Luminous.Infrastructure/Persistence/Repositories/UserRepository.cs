using Luminous.Domain.Interfaces;
using Luminous.Infrastructure.Persistence.Configuration;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using User = Luminous.Domain.Entities.User;

namespace Luminous.Infrastructure.Persistence.Repositories;

/// <summary>
/// Cosmos DB repository for User entities.
/// </summary>
public sealed class UserRepository : CosmosRepositoryBase<User>, IUserRepository
{
    public UserRepository(CosmosDbContext context, ILogger<UserRepository> logger)
        : base(context, logger, ContainerNames.Users)
    {
    }

    protected override PartitionKey GetPartitionKey(User entity) => new(entity.FamilyId);

    protected override string GetPartitionKeyPath() => "/familyId";

    public new async Task<User?> GetByIdAsync(string userId, string? familyId, CancellationToken cancellationToken = default)
    {
        return await base.GetByIdAsync(userId, familyId, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.email = @email")
            .WithParameter("@email", email.ToLowerInvariant());

        var results = await QueryAsync(query, cancellationToken: cancellationToken);
        return results.FirstOrDefault();
    }

    public async Task<User?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.externalId = @externalId")
            .WithParameter("@externalId", externalId);

        var results = await QueryAsync(query, cancellationToken: cancellationToken);
        return results.FirstOrDefault();
    }

    public async Task<IReadOnlyList<User>> GetByFamilyIdAsync(string familyId, CancellationToken cancellationToken = default)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.familyId = @familyId ORDER BY c.createdAt")
            .WithParameter("@familyId", familyId);

        return await QueryAsync(query, familyId, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        var query = new QueryDefinition("SELECT VALUE COUNT(1) FROM c WHERE c.email = @email")
            .WithParameter("@email", email.ToLowerInvariant());

        var container = await GetContainerAsync(cancellationToken);
        var iterator = container.GetItemQueryIterator<int>(query);
        var response = await iterator.ReadNextAsync(cancellationToken);

        return response.FirstOrDefault() > 0;
    }
}
