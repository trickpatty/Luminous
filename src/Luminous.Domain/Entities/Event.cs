using Luminous.Domain.Common;
using Luminous.Domain.Enums;
using Luminous.Domain.ValueObjects;

namespace Luminous.Domain.Entities;

/// <summary>
/// Represents a calendar event.
/// </summary>
public sealed class Event : Entity
{
    /// <summary>
    /// Gets or sets the family ID this event belongs to (partition key).
    /// </summary>
    public string FamilyId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the event title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the event description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the event start time (for timed events).
    /// Null for all-day events - use StartDate instead.
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// Gets or sets the event end time (for timed events).
    /// Null for all-day events - use EndTime instead.
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// Gets or sets the event start date (for all-day events).
    /// Null for timed events - use StartTime instead.
    /// </summary>
    public DateOnly? StartDate { get; set; }

    /// <summary>
    /// Gets or sets the event end date (for all-day events, exclusive).
    /// Null for timed events - use EndTime instead.
    /// For a single-day event, EndDate = StartDate + 1 day.
    /// </summary>
    public DateOnly? EndDate { get; set; }

    /// <summary>
    /// Gets or sets whether this is an all-day event.
    /// When true, StartDate/EndDate are used; when false, StartTime/EndTime are used.
    /// </summary>
    public bool IsAllDay { get; set; }

    /// <summary>
    /// Gets or sets the event location.
    /// </summary>
    public Address? Location { get; set; }

    /// <summary>
    /// Gets or sets the location description (freeform text).
    /// </summary>
    public string? LocationText { get; set; }

    /// <summary>
    /// Gets or sets the assigned family member IDs.
    /// </summary>
    public List<string> Assignees { get; set; } = [];

    /// <summary>
    /// Gets or sets the calendar provider this event came from.
    /// </summary>
    public CalendarProvider Source { get; set; } = CalendarProvider.Internal;

    /// <summary>
    /// Gets or sets the external calendar ID.
    /// </summary>
    public string? ExternalCalendarId { get; set; }

    /// <summary>
    /// Gets or sets the external event ID from the source calendar.
    /// </summary>
    public string? ExternalEventId { get; set; }

    /// <summary>
    /// Gets or sets the recurrence rule.
    /// </summary>
    public RecurrenceRule? Recurrence { get; set; }

    /// <summary>
    /// Gets or sets the parent recurring event ID (for instances).
    /// </summary>
    public string? RecurringEventId { get; set; }

    /// <summary>
    /// Gets or sets reminder minutes before the event.
    /// </summary>
    public List<int> Reminders { get; set; } = [15];

    /// <summary>
    /// Gets or sets the event color (hex code, overrides profile color).
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Gets whether this event is from an external source.
    /// </summary>
    public bool IsExternal => Source != CalendarProvider.Internal;

    /// <summary>
    /// Creates a new timed event.
    /// </summary>
    public static Event CreateTimed(
        string familyId,
        string title,
        DateTime startTime,
        DateTime endTime,
        string createdBy)
    {
        if (string.IsNullOrWhiteSpace(familyId))
            throw new ArgumentException("Family ID is required.", nameof(familyId));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));
        if (endTime < startTime)
            throw new ArgumentException("End time must be after start time.", nameof(endTime));

        return new Event
        {
            FamilyId = familyId,
            Title = title.Trim(),
            StartTime = startTime,
            EndTime = endTime,
            IsAllDay = false,
            Source = CalendarProvider.Internal,
            CreatedBy = createdBy
        };
    }

    /// <summary>
    /// Creates a new all-day event.
    /// </summary>
    /// <param name="familyId">The family ID.</param>
    /// <param name="title">The event title.</param>
    /// <param name="startDate">The start date (inclusive).</param>
    /// <param name="endDate">The end date (exclusive). For a single-day event, this should be startDate + 1 day.</param>
    /// <param name="createdBy">The creator's ID.</param>
    public static Event CreateAllDay(
        string familyId,
        string title,
        DateOnly startDate,
        DateOnly endDate,
        string createdBy)
    {
        if (string.IsNullOrWhiteSpace(familyId))
            throw new ArgumentException("Family ID is required.", nameof(familyId));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));
        if (endDate < startDate)
            throw new ArgumentException("End date must be on or after start date.", nameof(endDate));

        return new Event
        {
            FamilyId = familyId,
            Title = title.Trim(),
            StartDate = startDate,
            EndDate = endDate,
            IsAllDay = true,
            Source = CalendarProvider.Internal,
            CreatedBy = createdBy
        };
    }

    /// <summary>
    /// Creates a new internal event (legacy method for backward compatibility).
    /// </summary>
    [Obsolete("Use CreateTimed or CreateAllDay instead.")]
    public static Event Create(
        string familyId,
        string title,
        DateTime startTime,
        DateTime endTime,
        string createdBy,
        bool isAllDay = false)
    {
        if (isAllDay)
        {
            return CreateAllDay(
                familyId,
                title,
                DateOnly.FromDateTime(startTime),
                DateOnly.FromDateTime(endTime),
                createdBy);
        }

        return CreateTimed(familyId, title, startTime, endTime, createdBy);
    }
}
