using BikeTracking.Domain.Commands;
using BikeTracking.Domain.Entities;
using BikeTracking.Infrastructure.Data;
using BikeTracking.Infrastructure.Repositories;
using BikeTracking.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace bikeTracking.ApiService.Endpoints;

/// <summary>
/// Endpoints for ride management (T030).
/// POST /api/rides - Create a new ride with weather data.
/// </summary>
public static class RidesEndpoints
{
    public static void MapRidesEndpoints(this WebApplication app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/rides");

        _ = group.MapPost("/", CreateRideAsync)
            .RequireAuthorization()
            .WithName("CreateRide")
            .WithDescription("Create a new bike ride with weather data");

        _ = group.MapGet("/", GetUserRidesAsync)
            .RequireAuthorization()
            .WithName("GetUserRides")
            .WithDescription("Get user's rides with pagination");

        _ = group.MapGet("/{rideId}", GetRideDetailsAsync)

        _ = group.MapPut("/{rideId}", EditRideAsync)
            .RequireAuthorization()
            .WithName("EditRide")
            .WithDescription("Edit an existing ride with optional weather re-fetch");

        _ = group.MapDelete("/{rideId}", DeleteRideAsync)
            .RequireAuthorization()
            .WithName("DeleteRide")
            .WithDescription("Delete a ride if within 90-day window");
            .RequireAuthorization()
            .WithName("GetRideDetails")
            .WithDescription("Get full ride details including weather");
    }

    private static async Task<IResult> CreateRideAsync(
        [FromBody] CreateRideRequest request,
        [FromServices] CreateRideCommandHandler commandHandler,
        [FromServices] IEventStoreRepository eventRepository,
        [FromServices] IRideProjectionRepository projectionRepository,
        ClaimsPrincipal user)
    {
        try
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException("User ID not found");

            var rideId = Guid.NewGuid();

            // Execute command handler (pure function: generates events)
            var (rideCreatedEvent, additionalEvents) = await commandHandler.HandleAsync(
                rideId,
                userId,
                request.Date,
                request.Hour,
                request.Distance,
                request.DistanceUnit,
                request.RideName,
                request.StartLocation,
                request.EndLocation,
                request.Notes,
                request.Latitude,
                request.Longitude);

            // Append events to event store
            await eventRepository.AppendEventAsync(rideCreatedEvent);
            foreach (var @event in additionalEvents)
            {
                await eventRepository.AppendEventAsync(@event);
            }

            // Create read model projection
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
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500);
        }
    }

    private static async Task<IResult> GetUserRidesAsync(
        [FromServices] IRideProjectionRepository repository,
        ClaimsPrincipal user,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50
        )
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User ID not found");

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
        Guid rideId,
        [FromServices] IRideProjectionRepository repository,
        ClaimsPrincipal user)
    {
        var ride = await repository.GetByIdAsync(rideId);
        if (ride == null)
            return Results.NotFound();

        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (ride.UserId != userId)
            return Results.Forbid();

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
}


