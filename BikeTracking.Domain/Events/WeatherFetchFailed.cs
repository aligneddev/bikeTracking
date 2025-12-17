namespace BikeTracking.Domain.Events;

public class WeatherFetchFailed : DomainEvent
{
    public string? ErrorMessage { get; init; }
    public string SourceApi { get; init; } = "NOAA";
}
