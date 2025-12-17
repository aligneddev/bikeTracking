namespace BikeTracking.Domain.Events;

using BikeTracking.Domain.ValueObjects;

public class WeatherFetched : DomainEvent
{
    public Weather WeatherData { get; init; } = null!;
    public string SourceApi { get; init; } = "NOAA";
}
