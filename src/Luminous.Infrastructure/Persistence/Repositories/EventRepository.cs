using Luminous.Domain.Entities;
using Luminous.Domain.Interfaces;
using Luminous.Infrastructure.Persistence.Configuration;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace Luminous.Infrastructure.Persistence.Repositories;

/// <summary>
/// Cosmos DB repository for Event entities.
/// </summary>
public sealed class EventRepository : CosmosRepositoryBase<Event>, IEventRepository
{
    public EventRepository(CosmosDbContext context, ILogger<EventRepository> logger)
        : base(context, logger, ContainerNames.Events)
    {
    }

    protected override PartitionKey GetPartitionKey(Event entity) => new(entity.FamilyId);

    protected override string GetPartitionKeyPath() => "/familyId";

    public async Task<IReadOnlyList<Event>> GetByDateRangeAsync(
        string familyId,
        DateTime start,
        DateTime end,
        CancellationToken cancellationToken = default)
    {
        var query = new QueryDefinition(
            @"SELECT * FROM c
              WHERE c.familyId = @familyId
              AND ((c.startTime >= @start AND c.startTime < @end) OR (c.endTime > @start AND c.endTime <= @end))
              ORDER BY c.startTime")
            .WithParameter("@familyId", familyId)
            .WithParameter("@start", start)
            .WithParameter("@end", end);

        return await QueryAsync(query, familyId, cancellationToken);
    }

    public async Task<IReadOnlyList<Event>> GetByAssigneeAsync(
        string familyId,
        string assigneeId,
        DateTime start,
        DateTime end,
        CancellationToken cancellationToken = default)
    {
        var query = new QueryDefinition(
            @"SELECT * FROM c
              WHERE c.familyId = @familyId
              AND ARRAY_CONTAINS(c.assignees, @assigneeId)
              AND ((c.startTime >= @start AND c.startTime < @end) OR (c.endTime > @start AND c.endTime <= @end))
              ORDER BY c.startTime")
            .WithParameter("@familyId", familyId)
            .WithParameter("@assigneeId", assigneeId)
            .WithParameter("@start", start)
            .WithParameter("@end", end);

        return await QueryAsync(query, familyId, cancellationToken);
    }

    public async Task<IReadOnlyList<Event>> GetByExternalCalendarAsync(
        string familyId,
        string externalCalendarId,
        CancellationToken cancellationToken = default)
    {
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.familyId = @familyId AND c.externalCalendarId = @externalCalendarId")
            .WithParameter("@familyId", familyId)
            .WithParameter("@externalCalendarId", externalCalendarId);

        return await QueryAsync(query, familyId, cancellationToken);
    }

    public async Task<Event?> GetByExternalEventIdAsync(
        string familyId,
        string externalEventId,
        CancellationToken cancellationToken = default)
    {
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.familyId = @familyId AND c.externalEventId = @externalEventId")
            .WithParameter("@familyId", familyId)
            .WithParameter("@externalEventId", externalEventId);

        var results = await QueryAsync(query, familyId, cancellationToken);
        return results.FirstOrDefault();
    }

    public async Task<IReadOnlyList<Event>> GetUpcomingWithRemindersAsync(
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        var query = new QueryDefinition(
            @"SELECT * FROM c
              WHERE c.startTime >= @from AND c.startTime < @to
              AND ARRAY_LENGTH(c.reminders) > 0
              ORDER BY c.startTime")
            .WithParameter("@from", from)
            .WithParameter("@to", to);

        return await QueryAsync(query, cancellationToken: cancellationToken);
    }
}
