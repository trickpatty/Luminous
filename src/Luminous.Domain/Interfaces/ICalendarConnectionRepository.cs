using Luminous.Domain.Entities;
using Luminous.Domain.Enums;

namespace Luminous.Domain.Interfaces;

/// <summary>
/// Repository interface for CalendarConnection entities.
/// </summary>
public interface ICalendarConnectionRepository : IRepository<CalendarConnection>
{
    /// <summary>
    /// Gets all calendar connections for a family.
    /// </summary>
    Task<IReadOnlyList<CalendarConnection>> GetByFamilyAsync(
        string familyId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active calendar connections for a family.
    /// </summary>
    Task<IReadOnlyList<CalendarConnection>> GetActiveByFamilyAsync(
        string familyId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a calendar connection by external calendar ID.
    /// </summary>
    Task<CalendarConnection?> GetByExternalIdAsync(
        string familyId,
        string externalCalendarId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets calendar connections by provider type.
    /// </summary>
    Task<IReadOnlyList<CalendarConnection>> GetByProviderAsync(
        string familyId,
        CalendarProvider provider,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets calendar connections that need to be synced (NextSyncAt <= now).
    /// </summary>
    Task<IReadOnlyList<CalendarConnection>> GetDueSyncAsync(
        DateTime now,
        int limit = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets calendar connections that are in error state.
    /// </summary>
    Task<IReadOnlyList<CalendarConnection>> GetInErrorStateAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets calendar connections assigned to a specific member.
    /// </summary>
    Task<IReadOnlyList<CalendarConnection>> GetByAssignedMemberAsync(
        string familyId,
        string memberId,
        CancellationToken cancellationToken = default);
}
