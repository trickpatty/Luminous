using Nanoid;

namespace Luminous.Domain.Common;

/// <summary>
/// Base class for domain events.
/// </summary>
public abstract class DomainEvent
{
    /// <summary>
    /// Gets the unique identifier for this event.
    /// Uses NanoId for URL-friendly, compact unique identifiers.
    /// </summary>
    public string EventId { get; } = Nanoid.Generate();

    /// <summary>
    /// Gets the timestamp when this event occurred.
    /// </summary>
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
