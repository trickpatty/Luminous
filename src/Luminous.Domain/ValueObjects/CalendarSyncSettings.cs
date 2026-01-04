using Luminous.Domain.Common;

namespace Luminous.Domain.ValueObjects;

/// <summary>
/// Settings for calendar synchronization behavior.
/// </summary>
public sealed class CalendarSyncSettings : ValueObject
{
    /// <summary>
    /// Gets or sets how often to sync (in minutes). Default is 15 minutes.
    /// </summary>
    public int SyncIntervalMinutes { get; set; } = 15;

    /// <summary>
    /// Gets or sets how many days back to sync events. Default is 7 days.
    /// </summary>
    public int SyncPastDays { get; set; } = 7;

    /// <summary>
    /// Gets or sets how many days forward to sync events. Default is 90 days.
    /// </summary>
    public int SyncFutureDays { get; set; } = 90;

    /// <summary>
    /// Gets or sets whether to import all-day events.
    /// </summary>
    public bool ImportAllDayEvents { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to import declined events.
    /// </summary>
    public bool ImportDeclinedEvents { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to sync event descriptions.
    /// </summary>
    public bool SyncDescriptions { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to sync event locations.
    /// </summary>
    public bool SyncLocations { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to sync event reminders.
    /// </summary>
    public bool SyncReminders { get; set; } = true;

    /// <summary>
    /// Gets or sets whether this is a two-way sync (vs read-only).
    /// </summary>
    public bool TwoWaySync { get; set; } = false;

    /// <summary>
    /// Creates default sync settings.
    /// </summary>
    public static CalendarSyncSettings Default() => new();

    /// <summary>
    /// Creates settings for ICS URL subscriptions (read-only, hourly sync).
    /// </summary>
    public static CalendarSyncSettings ForIcsSubscription() => new()
    {
        SyncIntervalMinutes = 60,
        TwoWaySync = false,
        SyncPastDays = 7,
        SyncFutureDays = 180
    };

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return SyncIntervalMinutes;
        yield return SyncPastDays;
        yield return SyncFutureDays;
        yield return ImportAllDayEvents;
        yield return ImportDeclinedEvents;
        yield return SyncDescriptions;
        yield return SyncLocations;
        yield return SyncReminders;
        yield return TwoWaySync;
    }
}
