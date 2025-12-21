using Luminous.Domain.Common;

namespace Luminous.Domain.ValueObjects;

/// <summary>
/// Settings for display sleep mode.
/// </summary>
public sealed class SleepModeSettings : ValueObject
{
    /// <summary>
    /// Gets or sets whether sleep mode is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the time to start sleep mode.
    /// </summary>
    public TimeOnly StartTime { get; set; } = new(22, 0);

    /// <summary>
    /// Gets or sets the time to end sleep mode.
    /// </summary>
    public TimeOnly EndTime { get; set; } = new(7, 0);

    /// <summary>
    /// Gets or sets whether to wake on touch during sleep mode.
    /// </summary>
    public bool WakeOnTouch { get; set; } = true;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Enabled;
        yield return StartTime;
        yield return EndTime;
        yield return WakeOnTouch;
    }
}
