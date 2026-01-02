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
    new Task<Device?> GetByIdAsync(string deviceId, string? familyId, CancellationToken cancellationToken = default);

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

    /// <summary>
    /// Moves a device to a new partition (family). This is required when linking a device
    /// because CosmosDB partition keys cannot be updated in place.
    /// </summary>
    /// <param name="device">The device to move.</param>
    /// <param name="oldPartitionKey">The old partition key (device ID for unlinked devices).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<Device> MoveToFamilyAsync(Device device, string oldPartitionKey, CancellationToken cancellationToken = default);
}
