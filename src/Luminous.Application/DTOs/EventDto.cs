using Luminous.Domain.Enums;

namespace Luminous.Application.DTOs;

/// <summary>
/// Data transfer object for Event entity.
/// </summary>
public sealed record EventDto
{
    public string Id { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }

    /// <summary>
    /// Start time for timed events (ISO 8601 format). Null for all-day events.
    /// </summary>
    public DateTime? StartTime { get; init; }

    /// <summary>
    /// End time for timed events (ISO 8601 format). Null for all-day events.
    /// </summary>
    public DateTime? EndTime { get; init; }

    /// <summary>
    /// Start date for all-day events (YYYY-MM-DD format). Null for timed events.
    /// </summary>
    public string? StartDate { get; init; }

    /// <summary>
    /// End date for all-day events (YYYY-MM-DD format, exclusive). Null for timed events.
    /// </summary>
    public string? EndDate { get; init; }

    public bool IsAllDay { get; init; }
    public string? LocationText { get; init; }
    public List<UserRefDto> Assignees { get; init; } = [];
    public CalendarProvider Source { get; init; }
    public bool IsExternal { get; init; }
    public List<int> Reminders { get; init; } = [];
    public string? Color { get; init; }
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Simplified event DTO for calendar views.
/// </summary>
public sealed record EventSummaryDto
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Start time for timed events (ISO 8601 format). Null for all-day events.
    /// </summary>
    public DateTime? StartTime { get; init; }

    /// <summary>
    /// End time for timed events (ISO 8601 format). Null for all-day events.
    /// </summary>
    public DateTime? EndTime { get; init; }

    /// <summary>
    /// Start date for all-day events (YYYY-MM-DD format). Null for timed events.
    /// </summary>
    public string? StartDate { get; init; }

    /// <summary>
    /// End date for all-day events (YYYY-MM-DD format, exclusive). Null for timed events.
    /// </summary>
    public string? EndDate { get; init; }

    public bool IsAllDay { get; init; }
    public List<string> AssigneeIds { get; init; } = [];
    public string? Color { get; init; }
    public string? LocationText { get; init; }
}
