namespace BikeTracking.Domain.Commands;

using BikeTracking.Domain.Entities;
using BikeTracking.Domain.Events;
using BikeTracking.Domain.Results;
using BikeTracking.Domain.Services;
using BikeTracking.Domain.ValueObjects;

/// <summary>
/// Command handler for creating a new ride (T028).
/// Implements pure function pattern: converts request to domain events.
/// Handles weather fetching with graceful degradation.
/// </summary>
public class CreateRideCommandHandler
{
    private readonly IWeatherService _weatherService;

    public CreateRideCommandHandler(IWeatherService weatherService)
    {
        _weatherService = weatherService;
    }

    /// <summary>
    /// Handles ride creation command and generates domain events.
    /// Returns the RideCreated event plus any weather-related events.
    /// Uses functional Result pattern for error handling.
    /// </summary>
    /// <param name="rideId">New ride GUID</param>
    /// <param name="userId">User identity from OAuth token</param>
    /// <param name="date">Ride date (validated: today to -90 days)</param>
    /// <param name="hour">Hour of day (0-23)</param>
    /// <param name="distance">Distance traveled</param>
    /// <param name="distanceUnit">'miles' or 'kilometers'</param>
    /// <param name="rideName">User-provided ride name (max 200 chars)</param>
    /// <param name="startLocation">Start location name (max 200 chars)</param>
    /// <param name="endLocation">End location name (max 200 chars)</param>
    /// <param name="notes">Optional ride notes (max 1000 chars)</param>
    /// <param name="latitude">Location latitude for weather lookup (optional)</param>
    /// <param name="longitude">Location longitude for weather lookup (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing RideCreated event and any additional events (weather events)</returns>
    public async Task<Result<(RideCreated rideCreated, DomainEvent[] additionalEvents)>> HandleAsync(
        Guid rideId,
        string userId,
        DateOnly date,
        int hour,
        decimal distance,
        string distanceUnit,
        string rideName,
        string startLocation,
        string endLocation,
        string? notes,
        decimal? latitude,
        decimal? longitude,
        CancellationToken cancellationToken = default)
    {
        // Step 1: Create ride entity to validate business rules
        var ride = new Ride
        {
            RideId = rideId,
            UserId = userId,
            Date = date,
            Hour = hour,
            Distance = distance,
            DistanceUnit = distanceUnit,
            RideName = rideName,
            StartLocation = startLocation,
            EndLocation = endLocation,
            Notes = notes,
            CreatedTimestamp = DateTime.UtcNow
        };

        // Validate against domain constraints
        var validationResult = ride.Validate();
        if (validationResult is Result<Unit>.Failure failure)
        {
            return new Result<(RideCreated, DomainEvent[])>.Failure(failure.Error);
        }

        // Step 2: Fetch weather data with graceful degradation (FR-008)
        Weather? weatherData = null;
        var additionalEvents = new List<DomainEvent>();

        // Only fetch weather if coordinates provided
        if (latitude.HasValue && longitude.HasValue)
        {
            try
            {
                weatherData = await _weatherService.GetHistoricalWeatherAsync(
                    latitude.Value,
                    longitude.Value,
                    date,
                    hour,
                    cancellationToken);

                // If weather was fetched successfully (not all nulls)
                if (weatherData != null && !weatherData.IsUnavailable)
                {
                    var weatherFetchedEvent = new WeatherFetched
                    {
                        EventId = Guid.NewGuid(),
                        AggregateId = rideId,
                        AggregateType = nameof(Ride),
                        Timestamp = DateTime.UtcNow,
                        Version = 1,
                        UserId = userId,
                        WeatherData = weatherData,
                        SourceApi = "NOAA"
                    };
                    additionalEvents.Add(weatherFetchedEvent);
                }
                else
                {
                    // Weather API returned unavailable data (graceful degradation)
                    var weatherFailedEvent = new WeatherFetchFailed
                    {
                        EventId = Guid.NewGuid(),
                        AggregateId = rideId,
                        AggregateType = nameof(Ride),
                        Timestamp = DateTime.UtcNow,
                        Version = 2,
                        UserId = userId,
                        ErrorMessage = "Weather data unavailable - API returned null values",
                        SourceApi = "NOAA"
                    };
                    additionalEvents.Add(weatherFailedEvent);
                }
            }
            catch (Exception ex)
            {
                // Weather API call failed - log but continue (graceful degradation)
                var weatherFailedEvent = new WeatherFetchFailed
                {
                    EventId = Guid.NewGuid(),
                    AggregateId = rideId,
                    AggregateType = nameof(Ride),
                    Timestamp = DateTime.UtcNow,
                    Version = 2,
                    UserId = userId,
                    ErrorMessage = $"Weather fetch error: {ex.Message}",
                    SourceApi = "NOAA"
                };
                additionalEvents.Add(weatherFailedEvent);
                // Continue without weather - don't fail the ride creation
            }
        }

        // Step 3: Create RideCreated event (main aggregate event)
        var rideCreatedEvent = new RideCreated
        {
            EventId = Guid.NewGuid(),
            AggregateId = rideId,
            AggregateType = nameof(Ride),
            Timestamp = DateTime.UtcNow,
            Version = 0,
            UserId = userId,
            Date = date,
            Hour = hour,
            Distance = distance,
            DistanceUnit = distanceUnit,
            RideName = rideName,
            StartLocation = startLocation,
            EndLocation = endLocation,
            Notes = notes,
            WeatherData = weatherData // May be null (graceful degradation)
        };

        return new Result<(RideCreated, DomainEvent[])>.Success(
            (rideCreatedEvent, additionalEvents.ToArray()));
    }
}

