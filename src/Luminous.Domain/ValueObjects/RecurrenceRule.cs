using Luminous.Domain.Common;
using Luminous.Domain.Enums;

namespace Luminous.Domain.ValueObjects;

/// <summary>
/// Defines a recurrence pattern for events and chores.
/// </summary>
public sealed class RecurrenceRule : ValueObject
{
    /// <summary>
    /// Gets or sets the recurrence pattern type.
    /// </summary>
    public RecurrencePattern Pattern { get; set; } = RecurrencePattern.None;

    /// <summary>
    /// Gets or sets the interval between occurrences.
    /// </summary>
    public int Interval { get; set; } = 1;

    /// <summary>
    /// Gets or sets the days of week for weekly recurrence (0 = Sunday).
    /// </summary>
    public List<int> DaysOfWeek { get; set; } = [];

    /// <summary>
    /// Gets or sets the day of month for monthly recurrence.
    /// </summary>
    public int? DayOfMonth { get; set; }

    /// <summary>
    /// Gets or sets the end date for the recurrence.
    /// </summary>
    public DateOnly? EndDate { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of occurrences.
    /// </summary>
    public int? MaxOccurrences { get; set; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Pattern;
        yield return Interval;
        yield return string.Join(",", DaysOfWeek);
        yield return DayOfMonth;
        yield return EndDate;
        yield return MaxOccurrences;
    }
}
