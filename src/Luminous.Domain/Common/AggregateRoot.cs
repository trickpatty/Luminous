namespace Luminous.Domain.Common;

/// <summary>
/// Base class for aggregate roots in the domain model.
/// </summary>
public abstract class AggregateRoot : Entity
{
    /// <summary>
    /// Gets or sets the version for optimistic concurrency control.
    /// </summary>
    public string? ETag { get; set; }
}
