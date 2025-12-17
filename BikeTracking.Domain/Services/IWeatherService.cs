
using BikeTracking.Domain.ValueObjects;

namespace BikeTracking.Domain.Services;
/// <summary>
/// Interface for weather data retrieval service.
/// Supports historical hourly weather data with graceful degradation (FR-008).
/// </summary>
public interface IWeatherService
{
    /// <summary>
    /// Gets historical weather data for a location at a specific date and hour.
    /// Returns null-safe Weather object if data unavailable (graceful degradation).
    /// </summary>
    /// <param name="latitude">Location latitude</param>
    /// <param name="longitude">Location longitude</param>
    /// <param name="date">Ride date (within 90-day window)</param>
    /// <param name="hour">Hour of the day (0-23)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Weather data or object with all nulls if API fails</returns>
    Task<Weather?> GetHistoricalWeatherAsync(
        decimal latitude,
        decimal longitude,
        DateOnly date,
        int hour,
        CancellationToken cancellationToken = default);
}
