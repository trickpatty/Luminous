using Luminous.Domain.Common;
using Luminous.Domain.Enums;

namespace Luminous.Domain.Events;

/// <summary>
/// Event raised when a device link code is generated.
/// </summary>
public sealed class DeviceLinkCodeGeneratedEvent : DomainEvent
{
    public string DeviceId { get; }
    public string LinkCode { get; }
    public DateTime ExpiresAt { get; }

    public DeviceLinkCodeGeneratedEvent(string deviceId, string linkCode, DateTime expiresAt)
    {
        DeviceId = deviceId;
        LinkCode = linkCode;
        ExpiresAt = expiresAt;
    }
}

/// <summary>
/// Event raised when a device is linked to a family.
/// </summary>
public sealed class DeviceLinkedEvent : DomainEvent
{
    public string DeviceId { get; }
    public string FamilyId { get; }
    public string DeviceName { get; }
    public DeviceType DeviceType { get; }
    public string LinkedBy { get; }

    public DeviceLinkedEvent(string deviceId, string familyId, string deviceName, DeviceType deviceType, string linkedBy)
    {
        DeviceId = deviceId;
        FamilyId = familyId;
        DeviceName = deviceName;
        DeviceType = deviceType;
        LinkedBy = linkedBy;
    }
}

/// <summary>
/// Event raised when a device is unlinked from a family.
/// </summary>
public sealed class DeviceUnlinkedEvent : DomainEvent
{
    public string DeviceId { get; }
    public string FamilyId { get; }
    public string UnlinkedBy { get; }

    public DeviceUnlinkedEvent(string deviceId, string familyId, string unlinkedBy)
    {
        DeviceId = deviceId;
        FamilyId = familyId;
        UnlinkedBy = unlinkedBy;
    }
}
