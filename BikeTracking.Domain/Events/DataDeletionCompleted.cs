namespace BikeTracking.Domain.Events;

public class DataDeletionCompleted : DomainEvent
{
    public List<Guid>? DeletedRideIds { get; init; }
    public DateTime ProcessedTimestamp { get; init; }
}
