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
        // For unlinked devices, FamilyId is set to the device's own ID
        // For linked devices, FamilyId is the actual family ID
        return new PartitionKey(entity.FamilyId);
    }

    protected override string GetPartitionKeyPath() => "/familyId";

    public new async Task<Device?> GetByIdAsync(string deviceId, string? familyId, CancellationToken cancellationToken = default)
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

    public async Task<Device> MoveToFamilyAsync(Device device, string oldPartitionKey, CancellationToken cancellationToken = default)
    {
        var container = await GetContainerAsync(cancellationToken);

        // CosmosDB doesn't support changing partition keys, so we delete and recreate
        // Use a transactional batch to ensure atomicity within the same partition won't work here
        // since we're moving between partitions. We'll delete first, then create.

        // Delete the old document from the old partition
        await container.DeleteItemAsync<Device>(
            device.Id,
            new PartitionKey(oldPartitionKey),
            cancellationToken: cancellationToken);

        Logger.LogDebug("Deleted device {DeviceId} from old partition {OldPartition}", device.Id, oldPartitionKey);

        // Create the device in the new partition (with the new FamilyId)
        var response = await container.CreateItemAsync(
            device,
            GetPartitionKey(device),
            cancellationToken: cancellationToken);

        Logger.LogDebug("Created device {DeviceId} in new partition {NewPartition}", device.Id, device.FamilyId);

        return response.Resource;
    }
}
