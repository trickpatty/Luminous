using Luminous.Domain.Entities;
using Luminous.Domain.Interfaces;
using Luminous.Infrastructure.Persistence.Configuration;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace Luminous.Infrastructure.Persistence.Repositories;

/// <summary>
/// Cosmos DB repository for Family entities.
/// </summary>
public sealed class FamilyRepository : CosmosRepositoryBase<Family>, IFamilyRepository
{
    public FamilyRepository(CosmosDbContext context, ILogger<FamilyRepository> logger)
        : base(context, logger, ContainerNames.Families)
    {
    }

    protected override PartitionKey GetPartitionKey(Family entity) => new(entity.Id);

    protected override string GetPartitionKeyPath() => "/id";

    public async Task<Family?> GetByIdAsync(string familyId, CancellationToken cancellationToken = default)
    {
        return await GetByIdAsync(familyId, familyId, cancellationToken);
    }

    public async Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        var query = new QueryDefinition("SELECT VALUE COUNT(1) FROM c WHERE LOWER(c.name) = @name")
            .WithParameter("@name", name.ToLowerInvariant());

        var container = await GetContainerAsync(cancellationToken);
        var iterator = container.GetItemQueryIterator<int>(query);
        var response = await iterator.ReadNextAsync(cancellationToken);

        return response.FirstOrDefault() > 0;
    }
}
