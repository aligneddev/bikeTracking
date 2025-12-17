namespace BikeTracking.Domain.Events;

using BikeTracking.Domain.ValueObjects;

public class RideCreated : DomainEvent
{
    public DateOnly Date { get; init; }
    public int Hour { get; init; }
    public decimal Distance { get; init; }
    public string DistanceUnit { get; init; } = null!;
    public string RideName { get; init; } = null!;
    public string StartLocation { get; init; } = null!;
    public string EndLocation { get; init; } = null!;
    public string? Notes { get; init; }
    public Weather? WeatherData { get; init; }
}
