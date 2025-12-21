namespace Luminous.Domain.Common;

/// <summary>
/// Base class for domain events.
/// </summary>
public abstract class DomainEvent
{
    /// <summary>
    /// Gets the unique identifier for this event.
    /// </summary>
    public string EventId { get; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets the timestamp when this event occurred.
    /// </summary>
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
