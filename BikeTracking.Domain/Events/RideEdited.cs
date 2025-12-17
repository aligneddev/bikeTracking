namespace BikeTracking.Domain.Events;

using BikeTracking.Domain.ValueObjects;

/// <summary>
/// Domain event: Ride was edited (T040).
/// Captures all changed field names in JSON and new values.
/// </summary>
public class RideEdited : DomainEvent
{
    /// <summary>JSON array of field names that changed.</summary>
    public required string ChangedFields { get; init; }

    /// <summary>New date value if changed, else null.</summary>
    public DateOnly? NewDate { get; init; }

    /// <summary>New hour value if changed, else null.</summary>
    public int? NewHour { get; init; }

    /// <summary>New distance value if changed, else null.</summary>
    public decimal? NewDistance { get; init; }

    /// <summary>New ride name if changed, else null.</summary>
    public string? NewRideName { get; init; }

    /// <summary>New start location if changed, else null.</summary>
    public string? NewStartLocation { get; init; }

    /// <summary>New end location if changed, else null.</summary>
    public string? NewEndLocation { get; init; }

    /// <summary>New notes if changed, else null.</summary>
    public string? NewNotes { get; init; }

    /// <summary>New weather data if date/hour changed and successfully fetched.</summary>
    public Weather? NewWeatherData { get; init; }
}

