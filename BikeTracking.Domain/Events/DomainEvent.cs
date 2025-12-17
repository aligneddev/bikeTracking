namespace BikeTracking.Domain.Events;

/// <summary>
/// Base class for all domain events in event sourcing pattern.
/// Provides common properties for event tracking and audit trails.
/// </summary>
public abstract class DomainEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();

    public Guid AggregateId { get; set; }

    public string AggregateType { get; set; } = null!;

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public int Version { get; set; }

    public string UserId { get; set; } = null!;

    /// <summary>
    /// Gets the event type name for serialization/deserialization.
    /// </summary>
    public virtual string EventType => GetType().Name;
}
