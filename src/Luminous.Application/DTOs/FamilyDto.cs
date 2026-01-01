using Luminous.Domain.ValueObjects;

namespace Luminous.Application.DTOs;

/// <summary>
/// Data transfer object for Family entity.
/// </summary>
public sealed record FamilyDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Timezone { get; init; } = "UTC";
    public FamilySettingsDto Settings { get; init; } = new();
    public SubscriptionDto? Subscription { get; init; }
    public DateTime CreatedAt { get; init; }
    public int MemberCount { get; init; }
    public int DeviceCount { get; init; }
}

/// <summary>
/// Data transfer object for FamilySettings.
/// </summary>
public sealed record FamilySettingsDto
{
    public string DefaultView { get; init; } = "day";
    public bool PrivacyModeEnabled { get; init; } = true;
    public int PrivacyModeTimeoutMinutes { get; init; } = 5;
    public SleepModeSettingsDto SleepMode { get; init; } = new();
    public bool ShowWeather { get; init; } = true;
    public string? WeatherLocation { get; init; }
    public bool UseCelsius { get; init; }
    public int WeekStartDay { get; init; }
}

/// <summary>
/// Data transfer object for SleepModeSettings.
/// </summary>
public sealed record SleepModeSettingsDto
{
    public bool Enabled { get; init; }
    public string StartTime { get; init; } = "22:00";
    public string EndTime { get; init; } = "07:00";
    public bool WakeOnTouch { get; init; } = true;
}

/// <summary>
/// Data transfer object for SubscriptionInfo.
/// </summary>
public sealed record SubscriptionDto
{
    public string Tier { get; init; } = "free";
    public DateTime? StartedAt { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public bool IsActive { get; init; }
}

/// <summary>
/// Request DTO for updating family settings.
/// </summary>
public sealed record UpdateFamilySettingsRequestDto
{
    public string? Name { get; init; }
    public string? Timezone { get; init; }
    public FamilySettingsDto? Settings { get; init; }
}
