using Luminous.Domain.Entities;

namespace Luminous.Domain.Interfaces;

/// <summary>
/// Repository interface for Chore entities.
/// </summary>
public interface IChoreRepository : IRepository<Chore>
{
    /// <summary>
    /// Gets all chores for a family.
    /// </summary>
    Task<IReadOnlyList<Chore>> GetByFamilyIdAsync(
        string familyId,
        bool includeInactive = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets chores assigned to a specific user.
    /// </summary>
    Task<IReadOnlyList<Chore>> GetByAssigneeAsync(
        string familyId,
        string assigneeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets chores due on a specific date.
    /// </summary>
    Task<IReadOnlyList<Chore>> GetDueOnDateAsync(
        string familyId,
        DateOnly date,
        CancellationToken cancellationToken = default);
}
