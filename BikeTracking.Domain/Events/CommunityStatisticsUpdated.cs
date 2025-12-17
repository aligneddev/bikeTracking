namespace BikeTracking.Domain.Events;

public class CommunityStatisticsUpdated : DomainEvent
{
    public int TotalRides { get; init; }
    public decimal TotalDistance { get; init; }
    public decimal AverageDistance { get; init; }
    public DateTime UpdatedAt { get; init; }
}
