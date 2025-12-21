using Luminous.Domain.Enums;

namespace Luminous.Application.DTOs;

/// <summary>
/// Data transfer object for Device entity.
/// </summary>
public sealed record DeviceDto
{
    public string Id { get; init; } = string.Empty;
    public string FamilyId { get; init; } = string.Empty;
    public DeviceType Type { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsLinked { get; init; }
    public DateTime? LinkedAt { get; init; }
    public string? LinkedBy { get; init; }
    public DeviceSettingsDto Settings { get; init; } = new();
    public DateTime LastSeenAt { get; init; }
    public bool IsActive { get; init; }
    public string? Platform { get; init; }
    public string? AppVersion { get; init; }
}

/// <summary>
/// Data transfer object for DeviceSettings.
/// </summary>
public sealed record DeviceSettingsDto
{
    public string DefaultView { get; init; } = "day";
    public int Brightness { get; init; } = 80;
    public bool AutoBrightness { get; init; } = true;
    public string Orientation { get; init; } = "portrait";
    public bool SoundEnabled { get; init; } = true;
    public int Volume { get; init; } = 50;
}

/// <summary>
/// Data transfer object for device link code response.
/// </summary>
public sealed record DeviceLinkCodeDto
{
    public string DeviceId { get; init; } = string.Empty;
    public string LinkCode { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
}
