using Luminous.Domain.Common;

namespace Luminous.Domain.Events;

/// <summary>
/// Event raised when a chore is completed.
/// </summary>
public sealed class ChoreCompletedEvent : DomainEvent
{
    public string ChoreId { get; }
    public string FamilyId { get; }
    public string CompletedBy { get; }
    public int PointsEarned { get; }
    public DateOnly ForDate { get; }

    public ChoreCompletedEvent(string choreId, string familyId, string completedBy, int pointsEarned, DateOnly forDate)
    {
        ChoreId = choreId;
        FamilyId = familyId;
        CompletedBy = completedBy;
        PointsEarned = pointsEarned;
        ForDate = forDate;
    }
}

/// <summary>
/// Event raised when a routine is completed.
/// </summary>
public sealed class RoutineCompletedEvent : DomainEvent
{
    public string RoutineId { get; }
    public string FamilyId { get; }
    public string CompletedBy { get; }
    public int PointsEarned { get; }
    public DateOnly ForDate { get; }

    public RoutineCompletedEvent(string routineId, string familyId, string completedBy, int pointsEarned, DateOnly forDate)
    {
        RoutineId = routineId;
        FamilyId = familyId;
        CompletedBy = completedBy;
        PointsEarned = pointsEarned;
        ForDate = forDate;
    }
}
