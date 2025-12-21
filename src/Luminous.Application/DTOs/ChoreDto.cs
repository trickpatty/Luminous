using Luminous.Domain.Enums;

namespace Luminous.Application.DTOs;

/// <summary>
/// Data transfer object for Chore entity.
/// </summary>
public sealed record ChoreDto
{
    public string Id { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Icon { get; init; }
    public List<UserRefDto> Assignees { get; init; } = [];
    public bool IsForAnyone { get; init; }
    public RecurrenceRuleDto? Recurrence { get; init; }
    public string? DueTime { get; init; }
    public string? DueDate { get; init; }
    public int Points { get; init; }
    public int Priority { get; init; }
    public bool IsActive { get; init; }
    public int? EstimatedMinutes { get; init; }
    public string? Room { get; init; }
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// Data transfer object for RecurrenceRule.
/// </summary>
public sealed record RecurrenceRuleDto
{
    public RecurrencePattern Pattern { get; init; }
    public int Interval { get; init; } = 1;
    public List<int> DaysOfWeek { get; init; } = [];
    public int? DayOfMonth { get; init; }
    public string? EndDate { get; init; }
    public int? MaxOccurrences { get; init; }
}

/// <summary>
/// Data transfer object for Completion entity.
/// </summary>
public sealed record CompletionDto
{
    public string Id { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public string? ChoreId { get; init; }
    public string? RoutineId { get; init; }
    public UserRefDto CompletedBy { get; init; } = new();
    public DateTime CompletedAt { get; init; }
    public string ForDate { get; init; } = string.Empty;
    public int PointsEarned { get; init; }
    public UserRefDto? VerifiedBy { get; init; }
    public DateTime? VerifiedAt { get; init; }
    public string? Notes { get; init; }
}
