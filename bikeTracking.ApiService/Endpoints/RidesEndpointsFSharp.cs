using System.Security.Claims;

using BikeTracking.Api.Mapping;
using BikeTracking.Infrastructure.Repositories;
using BikeTracking.Shared.DTOs;

using static BikeTracking.Domain.FSharp.CommandHandlers;
using static BikeTracking.Domain.FSharp.Results;

namespace bikeTracking.ApiService.Endpoints;

/// <summary>
/// Endpoints for ride management with F# domain integration.
/// Maps between C# DTOs (API boundary) and F# domain types.
/// </summary>
public static class RidesEndpointsFSharp
{
    public static void MapRidesEndpointsFSharp(this WebApplication app)
    {
        var group = app.MapGroup("/api/v2/rides");

        _ = group.MapPost("/", CreateRideAsync).RequireAuthorization().WithName("CreateRideFSharp");
        _ = group
            .MapGet("/", GetUserRidesAsync)
            .RequireAuthorization()
            .WithName("GetUserRidesFSharp");
    }

    private static async Task<IResult> CreateRideAsync(
        CreateRideRequest request,
        CreateRideCommandHandler commandHandler,
        IRideProjectionRepository projectionRepository,
        ClaimsPrincipal user
    )
    {
        var userId =
            user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User ID not found");

        // DTO → F# Domain Command
        var command = DtoToDomainMapper.ToCreateRideCommand(request, userId);

        // Execute F# command handler
        var result = await commandHandler.HandleAsync(command, CancellationToken.None);

        // Pattern match on F# Result
        if (result is Result<CreateRideResult>.Success success)
        {
            var createResult = success.Item;

            // Extract ride data from F# event
            var rideCreated = createResult.RideCreated;
            if (rideCreated is BikeTracking.Domain.FSharp.Events.DomainEvent.RideCreated rc)
            {
                // Create C# projection for read model (convert F# types to C# strings)
                var projection = new BikeTracking.Domain.Entities.RideProjection
                {
                    RideId = command.RideId,
                    UserId = userId,
                    Date = rc.Item.Date,
                    Hour = rc.Item.Hour,
                    Distance = rc.Item.Distance,
                    DistanceUnit = DtoToDomainMapper.FromDistanceUnit(rc.Item.DistanceUnit),
                    RideName = rc.Item.RideName,
                    StartLocation = rc.Item.StartLocation,
                    EndLocation = rc.Item.EndLocation,
                    Notes = Microsoft.FSharp.Core.FSharpOption<string>.get_IsSome(rc.Item.Notes)
                        ? rc.Item.Notes.Value
                        : null,
                    WeatherData = ConvertFSharpWeatherToCSharp(rc.Item.WeatherData),
                    CreatedTimestamp = rc.Item.Metadata.Timestamp,
                    DeletionStatus = "active",
                    CommunityStatus = "private",
                    AgeInDays = 0,
                };

                var created = await projectionRepository.CreateAsync(projection);

                // F# Domain → DTO
                var response = DtoToDomainMapper.FromRideProjection(created);
                return Results.Created($"/api/v2/rides/{command.RideId}", response);
            }

            return Results.Problem("Unexpected event type");
        }

        if (result is Result<CreateRideResult>.Failure failure)
        {
            var error = failure.Item;
            return Results.BadRequest(
                new
                {
                    code = error.Code,
                    message = error.Message,
                    severity = error.Severity.ToString(),
                }
            );
        }

        return Results.Problem("Unexpected result type");
    }

    private static async Task<IResult> GetUserRidesAsync(
        IRideProjectionRepository projectionRepository,
        ClaimsPrincipal user,
        int page = 1,
        int pageSize = 20
    )
    {
        var userId =
            user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User ID not found");

        var projections = await projectionRepository.GetByUserIdAsync(userId, page, pageSize);

        // F# Domain → DTOs
        var response = projections.Select(DtoToDomainMapper.FromRideProjection).ToList();

        return Results.Ok(
            new RideListResponse
            {
                Data = response,
                Page = page,
                PageSize = pageSize,
                Total = response.Count,
            }
        );
    }

    private static BikeTracking.Domain.ValueObjects.Weather? ConvertFSharpWeatherToCSharp(
        Microsoft.FSharp.Core.FSharpOption<BikeTracking.Domain.FSharp.ValueObjects.Weather> fsharpWeather
    )
    {
        if (
            Microsoft.FSharp.Core.FSharpOption<BikeTracking.Domain.FSharp.ValueObjects.Weather>.get_IsNone(
                fsharpWeather
            )
        )
        {
            return null;
        }

        var w = fsharpWeather.Value;
        return new BikeTracking.Domain.ValueObjects.Weather
        {
            Temperature = Microsoft.FSharp.Core.FSharpOption<decimal>.get_IsSome(w.Temperature)
                ? w.Temperature.Value
                : null,
            Conditions = Microsoft.FSharp.Core.FSharpOption<string>.get_IsSome(w.Conditions)
                ? w.Conditions.Value
                : null,
            WindSpeed = Microsoft.FSharp.Core.FSharpOption<decimal>.get_IsSome(w.WindSpeed)
                ? w.WindSpeed.Value
                : null,
            WindDirection =
                Microsoft.FSharp.Core.FSharpOption<BikeTracking.Domain.FSharp.ValueObjects.WindDirection>.get_IsSome(
                    w.WindDirection
                )
                    ? w.WindDirection.Value.AsString()
                    : null,
            Humidity = Microsoft.FSharp.Core.FSharpOption<decimal>.get_IsSome(w.Humidity)
                ? w.Humidity.Value
                : null,
            Pressure = Microsoft.FSharp.Core.FSharpOption<decimal>.get_IsSome(w.Pressure)
                ? w.Pressure.Value
                : null,
            CapturedAt = w.CapturedAt,
        };
    }
}
