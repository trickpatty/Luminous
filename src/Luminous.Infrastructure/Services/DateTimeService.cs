using Luminous.Application.Common.Interfaces;

namespace Luminous.Infrastructure.Services;

/// <summary>
/// Implementation of the date/time service.
/// </summary>
public sealed class DateTimeService : IDateTimeService
{
    public DateTime UtcNow => DateTime.UtcNow;

    public DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);
}
