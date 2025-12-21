using Luminous.Domain.Common;
using Luminous.Domain.ValueObjects;

namespace Luminous.Domain.Entities;

/// <summary>
/// Represents a chore or task that can be assigned to family members.
/// </summary>
public sealed class Chore : Entity
{
    /// <summary>
    /// Gets or sets the family ID this chore belongs to (partition key).
    /// </summary>
    public string FamilyId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the chore title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the chore description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the icon identifier for this chore.
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Gets or sets the assigned family member IDs.
    /// Empty list means "anyone" can complete it.
    /// </summary>
    public List<string> Assignees { get; set; } = [];

    /// <summary>
    /// Gets or sets the recurrence rule.
    /// </summary>
    public RecurrenceRule? Recurrence { get; set; }

    /// <summary>
    /// Gets or sets the due time of day (for recurring chores).
    /// </summary>
    public TimeOnly? DueTime { get; set; }

    /// <summary>
    /// Gets or sets the specific due date (for one-time chores).
    /// </summary>
    public DateOnly? DueDate { get; set; }

    /// <summary>
    /// Gets or sets the points awarded for completing this chore.
    /// </summary>
    public int Points { get; set; }

    /// <summary>
    /// Gets or sets the priority level (1-5, higher is more important).
    /// </summary>
    public int Priority { get; set; } = 3;

    /// <summary>
    /// Gets or sets whether this chore is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the estimated duration in minutes.
    /// </summary>
    public int? EstimatedMinutes { get; set; }

    /// <summary>
    /// Gets or sets the room or area where this chore is performed.
    /// </summary>
    public string? Room { get; set; }

    /// <summary>
    /// Gets whether this chore can be completed by anyone.
    /// </summary>
    public bool IsForAnyone => Assignees.Count == 0;

    /// <summary>
    /// Creates a new chore.
    /// </summary>
    public static Chore Create(
        string familyId,
        string title,
        string createdBy,
        int points = 0,
        IEnumerable<string>? assignees = null)
    {
        if (string.IsNullOrWhiteSpace(familyId))
            throw new ArgumentException("Family ID is required.", nameof(familyId));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));

        return new Chore
        {
            FamilyId = familyId,
            Title = title.Trim(),
            Points = points,
            Assignees = assignees?.ToList() ?? [],
            CreatedBy = createdBy
        };
    }
}
