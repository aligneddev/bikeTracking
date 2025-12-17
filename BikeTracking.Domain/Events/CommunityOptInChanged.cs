namespace BikeTracking.Domain.Events;

public class CommunityOptInChanged : DomainEvent
{
    public bool OptInStatus { get; init; }
}
