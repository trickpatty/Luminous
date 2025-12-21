using Luminous.Domain.Common;

namespace Luminous.Domain.Entities;

/// <summary>
/// Represents a completion record for a chore or routine.
/// </summary>
public sealed class Completion : Entity
{
    /// <summary>
    /// Gets or sets the family ID (partition key).
    /// </summary>
    public string FamilyId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the chore ID (if completing a chore).
    /// </summary>
    public string? ChoreId { get; set; }

    /// <summary>
    /// Gets or sets the routine ID (if completing a routine).
    /// </summary>
    public string? RoutineId { get; set; }

    /// <summary>
    /// Gets or sets the user ID who completed the item.
    /// </summary>
    public string CompletedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the item was completed.
    /// </summary>
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date this completion was for (for recurring items).
    /// </summary>
    public DateOnly ForDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>
    /// Gets or sets the points earned for this completion.
    /// </summary>
    public int PointsEarned { get; set; }

    /// <summary>
    /// Gets or sets the user ID who verified this completion (if required).
    /// </summary>
    public string? VerifiedBy { get; set; }

    /// <summary>
    /// Gets or sets when the completion was verified.
    /// </summary>
    public DateTime? VerifiedAt { get; set; }

    /// <summary>
    /// Gets or sets any notes about this completion.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Gets or sets a photo URL proving completion.
    /// </summary>
    public string? PhotoUrl { get; set; }

    /// <summary>
    /// Gets whether this is a chore completion.
    /// </summary>
    public bool IsChoreCompletion => !string.IsNullOrEmpty(ChoreId);

    /// <summary>
    /// Gets whether this is a routine completion.
    /// </summary>
    public bool IsRoutineCompletion => !string.IsNullOrEmpty(RoutineId);

    /// <summary>
    /// Creates a chore completion record.
    /// </summary>
    public static Completion ForChore(string familyId, string choreId, string completedBy, int pointsEarned, DateOnly? forDate = null)
    {
        return new Completion
        {
            FamilyId = familyId,
            ChoreId = choreId,
            CompletedBy = completedBy,
            PointsEarned = pointsEarned,
            ForDate = forDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            CreatedBy = completedBy
        };
    }

    /// <summary>
    /// Creates a routine completion record.
    /// </summary>
    public static Completion ForRoutine(string familyId, string routineId, string completedBy, int pointsEarned, DateOnly? forDate = null)
    {
        return new Completion
        {
            FamilyId = familyId,
            RoutineId = routineId,
            CompletedBy = completedBy,
            PointsEarned = pointsEarned,
            ForDate = forDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            CreatedBy = completedBy
        };
    }
}
