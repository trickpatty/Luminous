using Luminous.Domain.Common;

namespace Luminous.Domain.Events;

/// <summary>
/// Event raised when a new family is created.
/// </summary>
public sealed class FamilyCreatedEvent : DomainEvent
{
    public string FamilyId { get; }
    public string FamilyName { get; }
    public string CreatedBy { get; }

    public FamilyCreatedEvent(string familyId, string familyName, string createdBy)
    {
        FamilyId = familyId;
        FamilyName = familyName;
        CreatedBy = createdBy;
    }
}

/// <summary>
/// Event raised when a family's settings are updated.
/// </summary>
public sealed class FamilySettingsUpdatedEvent : DomainEvent
{
    public string FamilyId { get; }
    public string UpdatedBy { get; }

    public FamilySettingsUpdatedEvent(string familyId, string updatedBy)
    {
        FamilyId = familyId;
        UpdatedBy = updatedBy;
    }
}

/// <summary>
/// Event raised when a family is deactivated.
/// </summary>
public sealed class FamilyDeactivatedEvent : DomainEvent
{
    public string FamilyId { get; }
    public string DeactivatedBy { get; }

    public FamilyDeactivatedEvent(string familyId, string deactivatedBy)
    {
        FamilyId = familyId;
        DeactivatedBy = deactivatedBy;
    }
}
