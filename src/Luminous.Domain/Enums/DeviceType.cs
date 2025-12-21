namespace Luminous.Domain.Enums;

/// <summary>
/// Defines the types of devices that can be linked to a family.
/// </summary>
public enum DeviceType
{
    /// <summary>
    /// Wall-mounted display device.
    /// </summary>
    Display = 0,

    /// <summary>
    /// Mobile device (iOS or Android app).
    /// </summary>
    Mobile = 1,

    /// <summary>
    /// Web browser client.
    /// </summary>
    Web = 2
}
