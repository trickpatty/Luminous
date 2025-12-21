using Luminous.Domain.Common;
using Luminous.Domain.ValueObjects;

namespace Luminous.Domain.Entities;

/// <summary>
/// Represents a multi-step routine for family members.
/// </summary>
public sealed class Routine : Entity
{
    /// <summary>
    /// Gets or sets the family ID this routine belongs to (partition key).
    /// </summary>
    public string FamilyId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the routine name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the routine description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the icon identifier for this routine.
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Gets or sets the assigned family member IDs.
    /// </summary>
    public List<string> Assignees { get; set; } = [];

    /// <summary>
    /// Gets or sets the steps in this routine.
    /// </summary>
    public List<RoutineStep> Steps { get; set; } = [];

    /// <summary>
    /// Gets or sets the recurrence rule.
    /// </summary>
    public RecurrenceRule? Recurrence { get; set; }

    /// <summary>
    /// Gets or sets the scheduled time for this routine.
    /// </summary>
    public TimeOnly? ScheduledTime { get; set; }

    /// <summary>
    /// Gets or sets the points awarded for completing this routine.
    /// </summary>
    public int Points { get; set; }

    /// <summary>
    /// Gets or sets whether this routine is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to play audio cues for step completion.
    /// </summary>
    public bool EnableAudioCues { get; set; }

    /// <summary>
    /// Gets the total estimated duration of all steps.
    /// </summary>
    public int TotalEstimatedMinutes => Steps.Sum(s => s.EstimatedMinutes ?? 0);

    /// <summary>
    /// Creates a new routine.
    /// </summary>
    public static Routine Create(
        string familyId,
        string name,
        string createdBy,
        IEnumerable<string>? assignees = null)
    {
        if (string.IsNullOrWhiteSpace(familyId))
            throw new ArgumentException("Family ID is required.", nameof(familyId));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        return new Routine
        {
            FamilyId = familyId,
            Name = name.Trim(),
            Assignees = assignees?.ToList() ?? [],
            CreatedBy = createdBy
        };
    }

    /// <summary>
    /// Adds a step to the routine.
    /// </summary>
    public void AddStep(string title, string? icon = null, int? estimatedMinutes = null)
    {
        var order = Steps.Count > 0 ? Steps.Max(s => s.Order) + 1 : 1;
        Steps.Add(new RoutineStep
        {
            Order = order,
            Title = title,
            Icon = icon,
            EstimatedMinutes = estimatedMinutes
        });
    }
}

/// <summary>
/// Represents a single step within a routine.
/// </summary>
public sealed class RoutineStep
{
    /// <summary>
    /// Gets or sets the step order.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Gets or sets the step title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the step description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the icon identifier.
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Gets or sets the image URL.
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Gets or sets the estimated duration in minutes.
    /// </summary>
    public int? EstimatedMinutes { get; set; }
}
