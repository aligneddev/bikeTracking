using System.Security.Claims;

using BikeTracking.Domain.Commands;
using BikeTracking.Domain.Entities;
using BikeTracking.Domain.Events;
using BikeTracking.Infrastructure.Repositories;
using BikeTracking.Shared.DTOs;

namespace bikeTracking.ApiService.Endpoints;

/// <summary>
/// Endpoints for ride management (T030, T045-T047).
/// </summary>
public static class RidesEndpoints
{
    public static void MapRidesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/rides");

        group.MapPost("/", CreateRideAsync).RequireAuthorization().WithName("CreateRide");
        group.MapGet("/", GetUserRidesAsync).RequireAuthorization().WithName("GetUserRides");
        group.MapGet("/{rideId}", GetRideDetailsAsync).RequireAuthorization().WithName("GetRideDetails");
        group.MapPut("/{rideId}", EditRideAsync).RequireAuthorization().WithName("EditRide");
        group.MapDelete("/{rideId}", DeleteRideAsync).RequireAuthorization().WithName("DeleteRide");
    }

    private static async Task<IResult> CreateRideAsync(
        CreateRideRequest request,
        CreateRideCommandHandler commandHandler,
        IEventStoreRepository eventRepository,
        IRideProjectionRepository projectionRepository,
        ClaimsPrincipal user)
    {
        try
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("User ID not found");
            var rideId = Guid.NewGuid();

            var (rideCreatedEvent, additionalEvents) = await commandHandler.HandleAsync(
                rideId, userId, request.Date, request.Hour, request.Distance, request.DistanceUnit,
                request.RideName, request.StartLocation, request.EndLocation, request.Notes, request.Latitude, request.Longitude);

            await eventRepository.AppendEventAsync(rideCreatedEvent);
            foreach (var @event in additionalEvents)
                await eventRepository.AppendEventAsync(@event);

            var projection = new RideProjection
            {
                RideId = rideId,
                UserId = userId,
                Date = rideCreatedEvent.Date,
                Hour = rideCreatedEvent.Hour,
                Distance = rideCreatedEvent.Distance,
                DistanceUnit = rideCreatedEvent.DistanceUnit,
                RideName = rideCreatedEvent.RideName,
                StartLocation = rideCreatedEvent.StartLocation,
                EndLocation = rideCreatedEvent.EndLocation,
                Notes = rideCreatedEvent.Notes,
                WeatherData = rideCreatedEvent.WeatherData,
                CreatedTimestamp = rideCreatedEvent.Timestamp,
                AgeInDays = 0
            };

            var created = await projectionRepository.CreateAsync(projection);

            var response = new RideResponse
            {
                RideId = created.RideId,
                UserId = created.UserId,
                Date = created.Date,
                Hour = created.Hour,
                Distance = created.Distance,
                DistanceUnit = created.DistanceUnit,
                RideName = created.RideName,
                StartLocation = created.StartLocation,
                EndLocation = created.EndLocation,
                Notes = created.Notes,
                Weather = created.WeatherData == null ? null : new WeatherResponse
                {
                    Temperature = created.WeatherData.Temperature,
                    Conditions = created.WeatherData.Conditions,
                    WindSpeed = created.WeatherData.WindSpeed,
                    WindDirection = created.WeatherData.WindDirection,
                    Humidity = created.WeatherData.Humidity,
                    Pressure = created.WeatherData.Pressure,
                    CapturedAt = created.WeatherData.CapturedAt
                },
                CreatedAt = created.CreatedTimestamp,
                AgeInDays = created.AgeInDays
            };

            return Results.Created($"/api/rides/{rideId}", response);
        }
        catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        catch (Exception ex) { return Results.Problem(detail: ex.Message, statusCode: 500); }
    }

    private static async Task<IResult> GetUserRidesAsync(
        IRideProjectionRepository repository, ClaimsPrincipal user, int page = 1, int pageSize = 50)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("User ID not found");
        var rides = await repository.GetByUserIdAsync(userId, page, pageSize);
        var count = await repository.GetUserRideCountAsync(userId);
        var responses = rides.Select(r => new RideListItemResponse
        {
            RideId = r.RideId,
            RideName = r.RideName,
            StartLocation = r.StartLocation,
            EndLocation = r.EndLocation,
            Distance = r.Distance,
            DistanceUnit = r.DistanceUnit,
            AgeInDays = r.AgeInDays,
            CreatedAt = r.CreatedTimestamp
        }).ToList();
        return Results.Ok(new { data = responses, total = count, page, pageSize });
    }

    private static async Task<IResult> GetRideDetailsAsync(
        Guid rideId, IRideProjectionRepository repository, ClaimsPrincipal user)
    {
        var ride = await repository.GetByIdAsync(rideId);
        if (ride == null) return Results.NotFound();

        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (ride.UserId != userId) return Results.Forbid();

        var response = new RideResponse
        {
            RideId = ride.RideId,
            UserId = ride.UserId,
            Date = ride.Date,
            Hour = ride.Hour,
            Distance = ride.Distance,
            DistanceUnit = ride.DistanceUnit,
            RideName = ride.RideName,
            StartLocation = ride.StartLocation,
            EndLocation = ride.EndLocation,
            Notes = ride.Notes,
            Weather = ride.WeatherData == null ? null : new WeatherResponse
            {
                Temperature = ride.WeatherData.Temperature,
                Conditions = ride.WeatherData.Conditions,
                WindSpeed = ride.WeatherData.WindSpeed,
                WindDirection = ride.WeatherData.WindDirection,
                Humidity = ride.WeatherData.Humidity,
                Pressure = ride.WeatherData.Pressure,
                CapturedAt = ride.WeatherData.CapturedAt
            },
            CreatedAt = ride.CreatedTimestamp,
            ModifiedAt = ride.ModifiedTimestamp,
            AgeInDays = ride.AgeInDays
        };
        return Results.Ok(response);
    }

    private static async Task<IResult> EditRideAsync(
        Guid rideId, EditRideRequest request, EditRideCommandHandler commandHandler,
        IEventStoreRepository eventRepository, IRideProjectionRepository projectionRepository, ClaimsPrincipal user)
    {
        try
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("User ID not found");
            var currentRide = await projectionRepository.GetByIdAsync(rideId);
            if (currentRide == null) return Results.NotFound();
            if (currentRide.UserId != userId) return Results.Forbid();

            (RideEdited rideEditedEvent, DomainEvent[] additionalEvents) = await commandHandler.HandleAsync(
                rideId, userId, currentRide, request.Date, request.Hour, request.Distance, request.DistanceUnit,
                request.RideName, request.StartLocation, request.EndLocation, request.Notes, request.Latitude, request.Longitude);

            await eventRepository.AppendEventAsync(rideEditedEvent);
            foreach (var @event in additionalEvents)
                await eventRepository.AppendEventAsync(@event);

            currentRide.Date = request.Date ?? currentRide.Date;
            currentRide.Hour = request.Hour ?? currentRide.Hour;
            currentRide.Distance = request.Distance ?? currentRide.Distance;
            currentRide.DistanceUnit = request.DistanceUnit ?? currentRide.DistanceUnit;
            currentRide.RideName = request.RideName ?? currentRide.RideName;
            currentRide.StartLocation = request.StartLocation ?? currentRide.StartLocation;
            currentRide.EndLocation = request.EndLocation ?? currentRide.EndLocation;
            currentRide.Notes = request.Notes ?? currentRide.Notes;
            if (rideEditedEvent.NewWeatherData != null) currentRide.WeatherData = rideEditedEvent.NewWeatherData;
            currentRide.ModifiedTimestamp = rideEditedEvent.Timestamp;

            await projectionRepository.UpdateAsync(currentRide);

            var response = new RideResponse
            {
                RideId = currentRide.RideId,
                UserId = currentRide.UserId,
                Date = currentRide.Date,
                Hour = currentRide.Hour,
                Distance = currentRide.Distance,
                DistanceUnit = currentRide.DistanceUnit,
                RideName = currentRide.RideName,
                StartLocation = currentRide.StartLocation,
                EndLocation = currentRide.EndLocation,
                Notes = currentRide.Notes,
                Weather = currentRide.WeatherData == null ? null : new WeatherResponse
                {
                    Temperature = currentRide.WeatherData.Temperature,
                    Conditions = currentRide.WeatherData.Conditions,
                    WindSpeed = currentRide.WeatherData.WindSpeed,
                    WindDirection = currentRide.WeatherData.WindDirection,
                    Humidity = currentRide.WeatherData.Humidity,
                    Pressure = currentRide.WeatherData.Pressure,
                    CapturedAt = currentRide.WeatherData.CapturedAt
                },
                CreatedAt = currentRide.CreatedTimestamp,
                ModifiedAt = currentRide.ModifiedTimestamp,
                AgeInDays = currentRide.AgeInDays
            };
            return Results.Ok(response);
        }
        catch (InvalidOperationException ex) { return Results.BadRequest(new { error = ex.Message }); }
        catch (Exception ex) { return Results.Problem(detail: ex.Message, statusCode: 500); }
    }

    private static async Task<IResult> DeleteRideAsync(
        Guid rideId, IRideProjectionRepository repository, ClaimsPrincipal user)
    {
        try
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException("User ID not found");
            var ride = await repository.GetByIdAsync(rideId);
            if (ride == null)
            {
                return Results.NotFound();
            }
            if (ride.UserId != userId)
            {
                return Results.Forbid();
            }
            if (ride.AgeInDays > 90)
            {
                return Results.BadRequest(new { error = "Cannot delete rides older than 90 days" });
            }

            await repository.DeleteAsync(rideId);
            return Results.NoContent();
        }
        catch (Exception ex) { return Results.Problem(detail: ex.Message, statusCode: 500); }
    }
}

