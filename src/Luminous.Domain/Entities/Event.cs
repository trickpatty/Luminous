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
    /// Gets or sets the event start time.
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Gets or sets the event end time.
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// Gets or sets whether this is an all-day event.
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
    /// Creates a new internal event.
    /// </summary>
    public static Event Create(
        string familyId,
        string title,
        DateTime startTime,
        DateTime endTime,
        string createdBy,
        bool isAllDay = false)
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
            IsAllDay = isAllDay,
            Source = CalendarProvider.Internal,
            CreatedBy = createdBy
        };
    }
}
