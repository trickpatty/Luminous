using Luminous.Domain.Entities;

namespace Luminous.Domain.Interfaces;

/// <summary>
/// Repository interface for Device entities.
/// </summary>
public interface IDeviceRepository : IRepository<Device>
{
    /// <summary>
    /// Gets a device by ID within a family.
    /// </summary>
    Task<Device?> GetByIdAsync(string deviceId, string familyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a device by its link code (for linking flow).
    /// </summary>
    Task<Device?> GetByLinkCodeAsync(string linkCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all devices in a family.
    /// </summary>
    Task<IReadOnlyList<Device>> GetByFamilyIdAsync(string familyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active devices that haven't been seen recently.
    /// </summary>
    Task<IReadOnlyList<Device>> GetInactiveDevicesAsync(TimeSpan inactivityThreshold, CancellationToken cancellationToken = default);
}
