namespace BikeTracking.Domain.ValueObjects;

/// <summary>
/// Immutable weather conditions at time of ride.
/// All fields nullable for graceful degradation when API data unavailable (FR-008).
/// </summary>
public class Weather
{
    public decimal? Temperature { get; init; }

    public string? Conditions { get; init; }

    public decimal? WindSpeed { get; init; }

    public string? WindDirection { get; init; }

    public decimal? Humidity { get; init; }

    public decimal? Pressure { get; init; }

    public DateTime CapturedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Returns true if weather data is unavailable (all fields null).
    /// </summary>
    public bool IsUnavailable =>
        Temperature is null &&
        Conditions is null &&
        WindSpeed is null &&
        WindDirection is null &&
        Humidity is null &&
        Pressure is null;

    /// <summary>
    /// Creates a Weather object with all fields set to null (graceful degradation).
    /// </summary>
    public static Weather CreateUnavailable() => new();
}
