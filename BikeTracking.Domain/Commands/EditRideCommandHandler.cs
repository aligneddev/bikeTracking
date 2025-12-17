using System.Text.Json;

using BikeTracking.Domain.Entities;
using BikeTracking.Domain.Events;
using BikeTracking.Domain.Services;
using BikeTracking.Domain.ValueObjects;

namespace BikeTracking.Domain.Commands;

/// <summary>
/// Command handler for editing an existing ride (T044).
/// Detects changed fields and fetches new weather if date/hour changed.
/// </summary>
public class EditRideCommandHandler(IWeatherService weatherService)
{
    private readonly IWeatherService _weatherService = weatherService;

    /// <summary>
    /// Handles ride edit command and generates RideEdited event.
    /// Re-fetches weather if date or hour changed.
    /// </summary>
    public async Task<(RideEdited rideEdited, DomainEvent[] additionalEvents)> HandleAsync(
        Guid rideId,
        string userId,
        RideProjection currentRide,
        DateOnly? newDate,
        int? newHour,
        decimal? newDistance,
        string? newDistanceUnit,
        string? newRideName,
        string? newStartLocation,
        string? newEndLocation,
        string? newNotes,
        decimal? latitude,
        decimal? longitude,
        CancellationToken cancellationToken = default
    )
    {
        // Detect which fields changed
        var changedFields = new List<string>();
        if (currentRide == null)
        {
            throw new ArgumentNullException(nameof(currentRide), "Current ride projection cannot be null.");
        }

        if (newDate.HasValue && newDate != currentRide.Date)
            changedFields.Add(nameof(currentRide.Date));
        if (newHour.HasValue && newHour != currentRide.Hour)
            changedFields.Add(nameof(currentRide.Hour));
        if (newDistance.HasValue && newDistance != currentRide.Distance)
            changedFields.Add(nameof(currentRide.Distance));
        if (newDistanceUnit != null && newDistanceUnit != currentRide.DistanceUnit)
            changedFields.Add(nameof(currentRide.DistanceUnit));
        if (newRideName != null && newRideName != currentRide.RideName)
            changedFields.Add(nameof(currentRide.RideName));
        if (newStartLocation != null && newStartLocation != currentRide.StartLocation)
            changedFields.Add(nameof(currentRide.StartLocation));
        if (newEndLocation != null && newEndLocation != currentRide.EndLocation)
            changedFields.Add(nameof(currentRide.EndLocation));
        if (newNotes != currentRide.Notes)
            changedFields.Add(nameof(currentRide.Notes));

        // Step 1: Create updated ride for validation
        var updatedRide = new Ride
        {
            RideId = rideId,
            UserId = userId,
            Date = newDate ?? currentRide.Date,
            Hour = newHour ?? currentRide.Hour,
            Distance = newDistance ?? currentRide.Distance,
            DistanceUnit = newDistanceUnit ?? currentRide.DistanceUnit,
            RideName = newRideName ?? currentRide.RideName,
            StartLocation = newStartLocation ?? currentRide.StartLocation,
            EndLocation = newEndLocation ?? currentRide.EndLocation,
            Notes = newNotes ?? currentRide.Notes,
            CreatedTimestamp = currentRide.CreatedTimestamp,
            ModifiedTimestamp = DateTime.UtcNow,
        };

        if (!updatedRide.IsValid(out var validationError))
        {
            throw new InvalidOperationException($"Ride validation failed: {validationError}");
        }

        // Step 2: Fetch new weather if date/hour changed
        Weather? newWeatherData = null;
        var additionalEvents = new List<DomainEvent>();

        bool dateOrHourChanged =
            (newDate.HasValue && newDate != currentRide.Date)
            || (newHour.HasValue && newHour != currentRide.Hour);

        if (dateOrHourChanged && latitude.HasValue && longitude.HasValue)
        {
            try
            {
                newWeatherData = await _weatherService.GetHistoricalWeatherAsync(
                    latitude.Value,
                    longitude.Value,
                    newDate ?? currentRide.Date,
                    newHour ?? currentRide.Hour,
                    cancellationToken
                );

                if (newWeatherData != null && !newWeatherData.IsUnavailable)
                {
                    additionalEvents.Add(
                        new WeatherFetched
                        {
                            EventId = Guid.NewGuid(),
                            AggregateId = rideId,
                            AggregateType = nameof(Ride),
                            Timestamp = DateTime.UtcNow,
                            Version = 2,
                            UserId = userId,
                            WeatherData = newWeatherData,
                        }
                    );
                }
                else
                {
                    additionalEvents.Add(
                        new WeatherFetchFailed
                        {
                            EventId = Guid.NewGuid(),
                            AggregateId = rideId,
                            AggregateType = nameof(Ride),
                            Timestamp = DateTime.UtcNow,
                            Version = 2,
                            UserId = userId,
                            ErrorMessage = "Updated weather data unavailable",
                        }
                    );
                }
            }
            catch (Exception ex)
            {
                additionalEvents.Add(
                    new WeatherFetchFailed
                    {
                        EventId = Guid.NewGuid(),
                        AggregateId = rideId,
                        AggregateType = nameof(Ride),
                        Timestamp = DateTime.UtcNow,
                        Version = 2,
                        UserId = userId,
                        ErrorMessage = $"Weather fetch error: {ex.Message}",
                    }
                );
            }
        }

        // Step 3: Create RideEdited event
        var rideEditedEvent = new RideEdited
        {
            EventId = Guid.NewGuid(),
            AggregateId = rideId,
            AggregateType = nameof(Ride),
            Timestamp = DateTime.UtcNow,
            Version = 1,
            UserId = userId,
            ChangedFields = JsonSerializer.Serialize(changedFields),
            NewDate = newDate,
            NewHour = newHour,
            NewDistance = newDistance,
            NewRideName = newRideName,
            NewStartLocation = newStartLocation,
            NewEndLocation = newEndLocation,
            NewNotes = newNotes,
            NewWeatherData = newWeatherData,
        };

        return (rideEditedEvent, additionalEvents.ToArray());
    }
}
