using Luminous.Domain.Common;
using Luminous.Domain.Enums;
using Luminous.Domain.ValueObjects;

namespace Luminous.Domain.Entities;

/// <summary>
/// Represents a device linked to a family.
/// </summary>
public sealed class Device : AggregateRoot
{
    /// <summary>
    /// Gets or sets the family ID this device belongs to (partition key).
    /// </summary>
    public string FamilyId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the device type.
    /// </summary>
    public DeviceType Type { get; set; } = DeviceType.Display;

    /// <summary>
    /// Gets or sets the device name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the 6-digit link code (when unlinked).
    /// </summary>
    public string? LinkCode { get; set; }

    /// <summary>
    /// Gets or sets when the link code expires.
    /// </summary>
    public DateTime? LinkCodeExpiry { get; set; }

    /// <summary>
    /// Gets or sets when the device was linked.
    /// </summary>
    public DateTime? LinkedAt { get; set; }

    /// <summary>
    /// Gets or sets the user ID who linked this device.
    /// </summary>
    public string? LinkedBy { get; set; }

    /// <summary>
    /// Gets or sets the device settings.
    /// </summary>
    public DeviceSettings Settings { get; set; } = new();

    /// <summary>
    /// Gets or sets the last time this device was seen online.
    /// </summary>
    public DateTime LastSeenAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets whether the device is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the device platform/OS information.
    /// </summary>
    public string? Platform { get; set; }

    /// <summary>
    /// Gets or sets the app version running on the device.
    /// </summary>
    public string? AppVersion { get; set; }

    /// <summary>
    /// Gets whether this device is currently linked.
    /// </summary>
    public bool IsLinked => LinkedAt != null;

    /// <summary>
    /// Gets whether the link code is still valid.
    /// </summary>
    public bool IsLinkCodeValid => !string.IsNullOrEmpty(LinkCode) && LinkCodeExpiry > DateTime.UtcNow;

    /// <summary>
    /// Creates a new unlinked device with a link code.
    /// </summary>
    public static Device CreateWithLinkCode(DeviceType type, string? platform = null)
    {
        var device = new Device
        {
            Type = type,
            Platform = platform,
            LinkCode = GenerateLinkCode(),
            LinkCodeExpiry = DateTime.UtcNow.AddMinutes(15)
        };

        return device;
    }

    /// <summary>
    /// Links this device to a family.
    /// </summary>
    public void Link(string familyId, string name, string linkedBy)
    {
        if (string.IsNullOrWhiteSpace(familyId))
            throw new ArgumentException("Family ID is required.", nameof(familyId));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Device name is required.", nameof(name));

        FamilyId = familyId;
        Name = name.Trim();
        LinkedAt = DateTime.UtcNow;
        LinkedBy = linkedBy;
        LinkCode = null;
        LinkCodeExpiry = null;
        ModifiedAt = DateTime.UtcNow;
        ModifiedBy = linkedBy;
    }

    /// <summary>
    /// Updates the last seen timestamp.
    /// </summary>
    public void RecordHeartbeat(string? appVersion = null)
    {
        LastSeenAt = DateTime.UtcNow;
        if (appVersion != null)
            AppVersion = appVersion;
    }

    /// <summary>
    /// Generates a new 6-digit link code.
    /// </summary>
    private static string GenerateLinkCode()
    {
        return Random.Shared.Next(100000, 999999).ToString();
    }
}
