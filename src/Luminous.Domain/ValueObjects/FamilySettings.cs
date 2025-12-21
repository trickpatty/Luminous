using Luminous.Domain.Common;

namespace Luminous.Domain.ValueObjects;

/// <summary>
/// Settings for a family household.
/// </summary>
public sealed class FamilySettings : ValueObject
{
    /// <summary>
    /// Gets or sets the default calendar view (day, week, month).
    /// </summary>
    public string DefaultView { get; set; } = "day";

    /// <summary>
    /// Gets or sets whether privacy mode is enabled.
    /// </summary>
    public bool PrivacyModeEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the timeout duration before entering privacy mode.
    /// </summary>
    public TimeSpan PrivacyModeTimeout { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets the sleep mode settings.
    /// </summary>
    public SleepModeSettings SleepMode { get; set; } = new();

    /// <summary>
    /// Gets or sets whether to show weather on the display.
    /// </summary>
    public bool ShowWeather { get; set; } = true;

    /// <summary>
    /// Gets or sets the weather location (address or coordinates).
    /// </summary>
    public string? WeatherLocation { get; set; }

    /// <summary>
    /// Gets or sets whether to use Celsius for temperature.
    /// </summary>
    public bool UseCelsius { get; set; }

    /// <summary>
    /// Gets or sets the week start day (0 = Sunday, 1 = Monday).
    /// </summary>
    public int WeekStartDay { get; set; } = 0;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return DefaultView;
        yield return PrivacyModeEnabled;
        yield return PrivacyModeTimeout;
        yield return SleepMode;
        yield return ShowWeather;
        yield return WeatherLocation;
        yield return UseCelsius;
        yield return WeekStartDay;
    }
}
