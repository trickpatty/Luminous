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
        // Convert DateTime to date strings for all-day event comparison
        // Use the local date from the UTC times (frontend sends local midnight as UTC)
        var startDateStr = DateOnly.FromDateTime(start).ToString("yyyy-MM-dd");
        var endDateStr = DateOnly.FromDateTime(end).ToString("yyyy-MM-dd");

        // Query for both timed events and all-day events:
        // - Timed events: compare startTime/endTime with the query range
        // - All-day events: compare startDate/endDate with the query date range
        // Note: Sorting is done in memory to avoid requiring a composite index in CosmosDB
        var query = new QueryDefinition(
            @"SELECT * FROM c
              WHERE c.familyId = @familyId
              AND (
                  (c.isAllDay = false AND (
                      (c.startTime >= @start AND c.startTime < @end)
                      OR (c.endTime > @start AND c.endTime <= @end)
                      OR (c.startTime <= @start AND c.endTime >= @end)
                  ))
                  OR
                  (c.isAllDay = true AND (
                      (c.startDate >= @startDate AND c.startDate < @endDate)
                      OR (c.endDate > @startDate AND c.endDate <= @endDate)
                      OR (c.startDate <= @startDate AND c.endDate >= @endDate)
                  ))
              )")
            .WithParameter("@familyId", familyId)
            .WithParameter("@start", start)
            .WithParameter("@end", end)
            .WithParameter("@startDate", startDateStr)
            .WithParameter("@endDate", endDateStr);

        var results = await QueryAsync(query, familyId, cancellationToken);

        // Sort in memory: all-day events first, then by date/time
        return results
            .OrderByDescending(e => e.IsAllDay)
            .ThenBy(e => e.StartDate)
            .ThenBy(e => e.StartTime)
            .ToList();
    }

    public async Task<IReadOnlyList<Event>> GetByAssigneeAsync(
        string familyId,
        string assigneeId,
        DateTime start,
        DateTime end,
        CancellationToken cancellationToken = default)
    {
        // Convert DateTime to date strings for all-day event comparison
        var startDateStr = DateOnly.FromDateTime(start).ToString("yyyy-MM-dd");
        var endDateStr = DateOnly.FromDateTime(end).ToString("yyyy-MM-dd");

        // Note: Sorting is done in memory to avoid requiring a composite index in CosmosDB
        var query = new QueryDefinition(
            @"SELECT * FROM c
              WHERE c.familyId = @familyId
              AND ARRAY_CONTAINS(c.assignees, @assigneeId)
              AND (
                  (c.isAllDay = false AND (
                      (c.startTime >= @start AND c.startTime < @end)
                      OR (c.endTime > @start AND c.endTime <= @end)
                      OR (c.startTime <= @start AND c.endTime >= @end)
                  ))
                  OR
                  (c.isAllDay = true AND (
                      (c.startDate >= @startDate AND c.startDate < @endDate)
                      OR (c.endDate > @startDate AND c.endDate <= @endDate)
                      OR (c.startDate <= @startDate AND c.endDate >= @endDate)
                  ))
              )")
            .WithParameter("@familyId", familyId)
            .WithParameter("@assigneeId", assigneeId)
            .WithParameter("@start", start)
            .WithParameter("@end", end)
            .WithParameter("@startDate", startDateStr)
            .WithParameter("@endDate", endDateStr);

        var results = await QueryAsync(query, familyId, cancellationToken);

        // Sort in memory: all-day events first, then by date/time
        return results
            .OrderByDescending(e => e.IsAllDay)
            .ThenBy(e => e.StartDate)
            .ThenBy(e => e.StartTime)
            .ToList();
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
