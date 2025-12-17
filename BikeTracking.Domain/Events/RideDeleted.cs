namespace BikeTracking.Domain.Events;

public class RideDeleted : DomainEvent
{
    public string DeletionType { get; init; } = null!; // "manual_3m" or "formal_request"
}
