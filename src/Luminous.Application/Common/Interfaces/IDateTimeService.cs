namespace Luminous.Application.Common.Interfaces;

/// <summary>
/// Service for accessing date/time (allows for testing).
/// </summary>
public interface IDateTimeService
{
    /// <summary>
    /// Gets the current UTC date and time.
    /// </summary>
    DateTime UtcNow { get; }

    /// <summary>
    /// Gets today's date in UTC.
    /// </summary>
    DateOnly Today { get; }
}
