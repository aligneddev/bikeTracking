namespace BikeTracking.Infrastructure.Data;

/// <summary>
/// Event store entity for event sourcing - all domain changes recorded here.
/// Provides immutable audit trail for compliance and reconstruction.
/// </summary>
public class Event
{
    public Guid EventId { get; set; }

    public Guid AggregateId { get; set; }

    public string AggregateType { get; set; } = null!;

    public string EventType { get; set; } = null!;

    public string EventData { get; set; } = null!; // JSON serialized event

    public DateTime Timestamp { get; set; }

    public int Version { get; set; }

    public string UserId { get; set; } = null!;
}
