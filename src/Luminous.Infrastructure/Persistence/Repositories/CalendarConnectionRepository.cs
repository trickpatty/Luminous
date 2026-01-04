using Luminous.Domain.Entities;
using Luminous.Domain.Enums;
using Luminous.Domain.Interfaces;
using Luminous.Infrastructure.Persistence.Configuration;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace Luminous.Infrastructure.Persistence.Repositories;

/// <summary>
/// Cosmos DB repository for CalendarConnection entities.
/// </summary>
public sealed class CalendarConnectionRepository : CosmosRepositoryBase<CalendarConnection>, ICalendarConnectionRepository
{
    public CalendarConnectionRepository(CosmosDbContext context, ILogger<CalendarConnectionRepository> logger)
        : base(context, logger, ContainerNames.CalendarConnections)
    {
    }

    protected override PartitionKey GetPartitionKey(CalendarConnection entity) => new(entity.FamilyId);

    protected override string GetPartitionKeyPath() => "/familyId";

    public async Task<IReadOnlyList<CalendarConnection>> GetByFamilyAsync(
        string familyId,
        CancellationToken cancellationToken = default)
    {
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.familyId = @familyId ORDER BY c.name")
            .WithParameter("@familyId", familyId);

        return await QueryAsync(query, familyId, cancellationToken);
    }

    public async Task<IReadOnlyList<CalendarConnection>> GetActiveByFamilyAsync(
        string familyId,
        CancellationToken cancellationToken = default)
    {
        var query = new QueryDefinition(
            @"SELECT * FROM c
              WHERE c.familyId = @familyId
              AND c.isEnabled = true
              AND c.status = @activeStatus
              ORDER BY c.name")
            .WithParameter("@familyId", familyId)
            .WithParameter("@activeStatus", (int)CalendarConnectionStatus.Active);

        return await QueryAsync(query, familyId, cancellationToken);
    }

    public async Task<CalendarConnection?> GetByExternalIdAsync(
        string familyId,
        string externalCalendarId,
        CancellationToken cancellationToken = default)
    {
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.familyId = @familyId AND c.externalCalendarId = @externalCalendarId")
            .WithParameter("@familyId", familyId)
            .WithParameter("@externalCalendarId", externalCalendarId);

        var results = await QueryAsync(query, familyId, cancellationToken);
        return results.FirstOrDefault();
    }

    public async Task<IReadOnlyList<CalendarConnection>> GetByProviderAsync(
        string familyId,
        CalendarProvider provider,
        CancellationToken cancellationToken = default)
    {
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.familyId = @familyId AND c.provider = @provider ORDER BY c.name")
            .WithParameter("@familyId", familyId)
            .WithParameter("@provider", (int)provider);

        return await QueryAsync(query, familyId, cancellationToken);
    }

    public async Task<IReadOnlyList<CalendarConnection>> GetDueSyncAsync(
        DateTime now,
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        // Cross-partition query to find connections that need syncing
        var query = new QueryDefinition(
            @"SELECT TOP @limit * FROM c
              WHERE c.isEnabled = true
              AND c.status = @activeStatus
              AND c.nextSyncAt <= @now
              ORDER BY c.nextSyncAt")
            .WithParameter("@limit", limit)
            .WithParameter("@activeStatus", (int)CalendarConnectionStatus.Active)
            .WithParameter("@now", now);

        return await QueryAsync(query, cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<CalendarConnection>> GetInErrorStateAsync(
        CancellationToken cancellationToken = default)
    {
        // Cross-partition query to find connections in error state
        var query = new QueryDefinition(
            @"SELECT * FROM c
              WHERE c.status IN (@authError, @syncError)
              ORDER BY c.modifiedAt DESC")
            .WithParameter("@authError", (int)CalendarConnectionStatus.AuthError)
            .WithParameter("@syncError", (int)CalendarConnectionStatus.SyncError);

        return await QueryAsync(query, cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<CalendarConnection>> GetByAssignedMemberAsync(
        string familyId,
        string memberId,
        CancellationToken cancellationToken = default)
    {
        var query = new QueryDefinition(
            @"SELECT * FROM c
              WHERE c.familyId = @familyId
              AND ARRAY_CONTAINS(c.assignedMemberIds, @memberId)
              ORDER BY c.name")
            .WithParameter("@familyId", familyId)
            .WithParameter("@memberId", memberId);

        return await QueryAsync(query, familyId, cancellationToken);
    }
}
