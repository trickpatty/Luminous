using Luminous.Domain.Common;

namespace Luminous.Domain.ValueObjects;

/// <summary>
/// Settings for a linked device.
/// </summary>
public sealed class DeviceSettings : ValueObject
{
    /// <summary>
    /// Gets or sets the default view for this device.
    /// </summary>
    public string DefaultView { get; set; } = "day";

    /// <summary>
    /// Gets or sets the brightness level (0-100).
    /// </summary>
    public int Brightness { get; set; } = 80;

    /// <summary>
    /// Gets or sets whether to use auto-brightness.
    /// </summary>
    public bool AutoBrightness { get; set; } = true;

    /// <summary>
    /// Gets or sets the display orientation (landscape, portrait).
    /// </summary>
    public string Orientation { get; set; } = "portrait";

    /// <summary>
    /// Gets or sets whether to enable sound effects.
    /// </summary>
    public bool SoundEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the volume level (0-100).
    /// </summary>
    public int Volume { get; set; } = 50;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return DefaultView;
        yield return Brightness;
        yield return AutoBrightness;
        yield return Orientation;
        yield return SoundEnabled;
        yield return Volume;
    }
}
