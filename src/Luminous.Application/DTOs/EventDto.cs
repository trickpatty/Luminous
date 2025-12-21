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
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
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
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    public bool IsAllDay { get; init; }
    public List<string> AssigneeIds { get; init; } = [];
    public string? Color { get; init; }
}
