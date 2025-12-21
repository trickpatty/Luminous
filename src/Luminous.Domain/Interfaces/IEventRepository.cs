using Luminous.Domain.Entities;

namespace Luminous.Domain.Interfaces;

/// <summary>
/// Repository interface for Event entities.
/// </summary>
public interface IEventRepository : IRepository<Event>
{
    /// <summary>
    /// Gets events for a family within a date range.
    /// </summary>
    Task<IReadOnlyList<Event>> GetByDateRangeAsync(
        string familyId,
        DateTime start,
        DateTime end,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets events for specific assignees within a date range.
    /// </summary>
    Task<IReadOnlyList<Event>> GetByAssigneeAsync(
        string familyId,
        string assigneeId,
        DateTime start,
        DateTime end,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets events by external calendar ID.
    /// </summary>
    Task<IReadOnlyList<Event>> GetByExternalCalendarAsync(
        string familyId,
        string externalCalendarId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an event by its external event ID.
    /// </summary>
    Task<Event?> GetByExternalEventIdAsync(
        string familyId,
        string externalEventId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets upcoming events that need reminders.
    /// </summary>
    Task<IReadOnlyList<Event>> GetUpcomingWithRemindersAsync(
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);
}
