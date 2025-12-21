namespace Luminous.Domain.Enums;

/// <summary>
/// Defines the recurrence patterns for chores and events.
/// </summary>
public enum RecurrencePattern
{
    /// <summary>
    /// No recurrence - one-time item.
    /// </summary>
    None = 0,

    /// <summary>
    /// Repeats daily.
    /// </summary>
    Daily = 1,

    /// <summary>
    /// Repeats weekly on specified days.
    /// </summary>
    Weekly = 2,

    /// <summary>
    /// Repeats monthly on a specified day.
    /// </summary>
    Monthly = 3,

    /// <summary>
    /// Repeats yearly on a specified date.
    /// </summary>
    Yearly = 4,

    /// <summary>
    /// Custom recurrence pattern.
    /// </summary>
    Custom = 5
}
