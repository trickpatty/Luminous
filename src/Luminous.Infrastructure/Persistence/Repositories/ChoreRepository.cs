using Luminous.Domain.Entities;
using Luminous.Domain.Interfaces;
using Luminous.Infrastructure.Persistence.Configuration;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace Luminous.Infrastructure.Persistence.Repositories;

/// <summary>
/// Cosmos DB repository for Chore entities.
/// </summary>
public sealed class ChoreRepository : CosmosRepositoryBase<Chore>, IChoreRepository
{
    public ChoreRepository(CosmosDbContext context, ILogger<ChoreRepository> logger)
        : base(context, logger, ContainerNames.Chores)
    {
    }

    protected override PartitionKey GetPartitionKey(Chore entity) => new(entity.FamilyId);

    protected override string GetPartitionKeyPath() => "/familyId";

    public async Task<IReadOnlyList<Chore>> GetByFamilyIdAsync(
        string familyId,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var queryText = includeInactive
            ? "SELECT * FROM c WHERE c.familyId = @familyId ORDER BY c.priority DESC, c.title"
            : "SELECT * FROM c WHERE c.familyId = @familyId AND c.isActive = true ORDER BY c.priority DESC, c.title";

        var query = new QueryDefinition(queryText)
            .WithParameter("@familyId", familyId);

        return await QueryAsync(query, familyId, cancellationToken);
    }

    public async Task<IReadOnlyList<Chore>> GetByAssigneeAsync(
        string familyId,
        string assigneeId,
        CancellationToken cancellationToken = default)
    {
        var query = new QueryDefinition(
            @"SELECT * FROM c
              WHERE c.familyId = @familyId
              AND c.isActive = true
              AND (ARRAY_LENGTH(c.assignees) = 0 OR ARRAY_CONTAINS(c.assignees, @assigneeId))
              ORDER BY c.priority DESC, c.title")
            .WithParameter("@familyId", familyId)
            .WithParameter("@assigneeId", assigneeId);

        return await QueryAsync(query, familyId, cancellationToken);
    }

    public async Task<IReadOnlyList<Chore>> GetDueOnDateAsync(
        string familyId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        // Get recurring chores that apply to this day of week
        var dayOfWeek = (int)date.DayOfWeek;

        var query = new QueryDefinition(
            @"SELECT * FROM c
              WHERE c.familyId = @familyId
              AND c.isActive = true
              AND (
                  c.dueDate = @date
                  OR (c.recurrence.pattern = 1)
                  OR (c.recurrence.pattern = 2 AND ARRAY_CONTAINS(c.recurrence.daysOfWeek, @dayOfWeek))
                  OR (c.recurrence.pattern = 3 AND c.recurrence.dayOfMonth = @dayOfMonth)
              )
              ORDER BY c.dueTime, c.priority DESC")
            .WithParameter("@familyId", familyId)
            .WithParameter("@date", date.ToString("yyyy-MM-dd"))
            .WithParameter("@dayOfWeek", dayOfWeek)
            .WithParameter("@dayOfMonth", date.Day);

        return await QueryAsync(query, familyId, cancellationToken);
    }
}
