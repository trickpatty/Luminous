using Luminous.Domain.Entities;
using Luminous.Domain.Interfaces;
using Luminous.Infrastructure.Persistence.Configuration;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace Luminous.Infrastructure.Persistence.Repositories;

/// <summary>
/// Cosmos DB repository for Device entities.
/// </summary>
public sealed class DeviceRepository : CosmosRepositoryBase<Device>, IDeviceRepository
{
    public DeviceRepository(CosmosDbContext context, ILogger<DeviceRepository> logger)
        : base(context, logger, ContainerNames.Devices)
    {
    }

    protected override PartitionKey GetPartitionKey(Device entity)
    {
        // Unlinked devices don't have a family ID yet
        return string.IsNullOrEmpty(entity.FamilyId)
            ? new PartitionKey(entity.Id)
            : new PartitionKey(entity.FamilyId);
    }

    protected override string GetPartitionKeyPath() => "/familyId";

    public new async Task<Device?> GetByIdAsync(string deviceId, string familyId, CancellationToken cancellationToken = default)
    {
        return await base.GetByIdAsync(deviceId, familyId, cancellationToken);
    }

    public async Task<Device?> GetByLinkCodeAsync(string linkCode, CancellationToken cancellationToken = default)
    {
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.linkCode = @linkCode AND c.linkCodeExpiry > @now")
            .WithParameter("@linkCode", linkCode)
            .WithParameter("@now", DateTime.UtcNow);

        var results = await QueryAsync(query, cancellationToken: cancellationToken);
        return results.FirstOrDefault();
    }

    public async Task<IReadOnlyList<Device>> GetByFamilyIdAsync(string familyId, CancellationToken cancellationToken = default)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.familyId = @familyId ORDER BY c.name")
            .WithParameter("@familyId", familyId);

        return await QueryAsync(query, familyId, cancellationToken);
    }

    public async Task<IReadOnlyList<Device>> GetInactiveDevicesAsync(TimeSpan inactivityThreshold, CancellationToken cancellationToken = default)
    {
        var threshold = DateTime.UtcNow - inactivityThreshold;

        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.isActive = true AND c.lastSeenAt < @threshold")
            .WithParameter("@threshold", threshold);

        return await QueryAsync(query, cancellationToken: cancellationToken);
    }
}
