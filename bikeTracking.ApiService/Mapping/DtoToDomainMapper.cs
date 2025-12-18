using BikeTracking.Shared.DTOs;
using static BikeTracking.Domain.FSharp.CommandHandlers;
using static BikeTracking.Domain.FSharp.Entities;
using static BikeTracking.Domain.FSharp.ValueObjects;

namespace BikeTracking.Api.Mapping;
/// <summary>
/// Maps between C# DTOs (API boundary) and F# domain types.
/// Keeps Blazor WASM lightweight by avoiding F# types on client.
/// </summary>
public static class DtoToDomainMapper
{
    // ==================== Value Object Mappings ====================

    public static DistanceUnit ToDistanceUnit(string dto)
    {
        return dto.ToLowerInvariant() switch
        {
            "miles" => DistanceUnit.Miles,
            "kilometers" => DistanceUnit.Kilometers,
            _ => throw new ArgumentException($"Invalid distance unit: {dto}", nameof(dto)),
        };
    }

    public static string FromDistanceUnit(DistanceUnit unit)
    {
        return unit.AsString();
    }

    public static DeletionStatus ToDeletionStatus(string dto)
    {
        return dto.ToLowerInvariant() switch
        {
            "active" => DeletionStatus.Active,
            "marked_for_deletion" => DeletionStatus.MarkedForDeletion,
            _ => throw new ArgumentException($"Invalid deletion status: {dto}", nameof(dto)),
        };
    }

    public static string FromDeletionStatus(DeletionStatus status)
    {
        return status.AsString();
    }

    public static CommunityStatus ToCommunityStatus(string dto)
    {
        return dto.ToLowerInvariant() switch
        {
            "private" => CommunityStatus.Private,
            "shareable" => CommunityStatus.Shareable,
            "public" => CommunityStatus.Public,
            _ => throw new ArgumentException($"Invalid community status: {dto}", nameof(dto)),
        };
    }

    public static string FromCommunityStatus(CommunityStatus status)
    {
        return status.AsString();
    }

    public static WindDirection? ToWindDirection(string? dto)
    {
        if (string.IsNullOrEmpty(dto))
            return null;

        return dto.ToUpperInvariant() switch
        {
            "N" or "NORTH" => WindDirection.North,
            "NE" or "NORTHEAST" => WindDirection.NorthEast,
            "E" or "EAST" => WindDirection.East,
            "SE" or "SOUTHEAST" => WindDirection.SouthEast,
            "S" or "SOUTH" => WindDirection.South,
            "SW" or "SOUTHWEST" => WindDirection.SouthWest,
            "W" or "WEST" => WindDirection.West,
            "NW" or "NORTHWEST" => WindDirection.NorthWest,
            _ => null,
        };
    }

    public static string? FromWindDirection(
        Microsoft.FSharp.Core.FSharpOption<WindDirection> windDir
    )
    {
        if (
            Microsoft.FSharp.Core.FSharpOption<WindDirection>.get_IsNone(windDir)
            || windDir?.Value == null
        )
        {
            return null;
        }

        var direction = windDir.Value;
        return direction.AsString();
    }

    // ==================== Weather Mapping ====================

    public static Weather? ToWeather(WeatherResponse? dto)
    {
        if (dto == null)
        {
            return null;
        }

        return new Weather(
            Microsoft.FSharp.Core.FSharpOption<decimal>.Some(dto.Temperature ?? 0m),
            dto.Conditions != null
                ? Microsoft.FSharp.Core.FSharpOption<string>.Some(dto.Conditions)
                : Microsoft.FSharp.Core.FSharpOption<string>.None,
            dto.WindSpeed.HasValue
                ? Microsoft.FSharp.Core.FSharpOption<decimal>.Some(dto.WindSpeed.Value)
                : Microsoft.FSharp.Core.FSharpOption<decimal>.None,
            ToWindDirection(dto.WindDirection) != null
                ? Microsoft.FSharp.Core.FSharpOption<WindDirection>.Some(
                    ToWindDirection(dto.WindDirection)!
                )
                : Microsoft.FSharp.Core.FSharpOption<WindDirection>.None,
            dto.Humidity.HasValue
                ? Microsoft.FSharp.Core.FSharpOption<decimal>.Some(dto.Humidity.Value)
                : Microsoft.FSharp.Core.FSharpOption<decimal>.None,
            dto.Pressure.HasValue
                ? Microsoft.FSharp.Core.FSharpOption<decimal>.Some(dto.Pressure.Value)
                : Microsoft.FSharp.Core.FSharpOption<decimal>.None,
            dto.CapturedAt
        );
    }

    public static WeatherResponse? FromWeather(Microsoft.FSharp.Core.FSharpOption<Weather> weather)
    {
        if (
            Microsoft.FSharp.Core.FSharpOption<Weather>.get_IsNone(weather)
            || weather?.Value == null
        )
        {
            return null;
        }

        var w = weather.Value;
        return new WeatherResponse
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
            WindDirection = FromWindDirection(w.WindDirection),
            Humidity = Microsoft.FSharp.Core.FSharpOption<decimal>.get_IsSome(w.Humidity)
                ? w.Humidity.Value
                : null,
            Pressure = Microsoft.FSharp.Core.FSharpOption<decimal>.get_IsSome(w.Pressure)
                ? w.Pressure.Value
                : null,
            CapturedAt = w.CapturedAt,
        };
    }

    // ==================== Command Mappings ====================

    public static CreateRideCommand ToCreateRideCommand(CreateRideRequest dto, string userId)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return new CreateRideCommand(
            Guid.NewGuid(),
            userId,
            dto.Date,
            dto.Hour,
            dto.Distance,
            ToDistanceUnit(dto.DistanceUnit),
            dto.RideName,
            dto.StartLocation,
            dto.EndLocation,
            dto.Notes != null
                ? Microsoft.FSharp.Core.FSharpOption<string>.Some(dto.Notes)
                : Microsoft.FSharp.Core.FSharpOption<string>.None,
            dto.Latitude.HasValue
                ? Microsoft.FSharp.Core.FSharpOption<decimal>.Some(dto.Latitude.Value)
                : Microsoft.FSharp.Core.FSharpOption<decimal>.None,
            dto.Longitude.HasValue
                ? Microsoft.FSharp.Core.FSharpOption<decimal>.Some(dto.Longitude.Value)
                : Microsoft.FSharp.Core.FSharpOption<decimal>.None
        );
    }

    public static EditRideCommand ToEditRideCommand(EditRideRequest dto, Guid rideId, string userId)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return new EditRideCommand(
            rideId,
            userId,
            dto.Date.HasValue
                ? Microsoft.FSharp.Core.FSharpOption<DateOnly>.Some(dto.Date.Value)
                : Microsoft.FSharp.Core.FSharpOption<DateOnly>.None,
            dto.Hour.HasValue
                ? Microsoft.FSharp.Core.FSharpOption<int>.Some(dto.Hour.Value)
                : Microsoft.FSharp.Core.FSharpOption<int>.None,
            dto.Distance.HasValue
                ? Microsoft.FSharp.Core.FSharpOption<decimal>.Some(dto.Distance.Value)
                : Microsoft.FSharp.Core.FSharpOption<decimal>.None,
            dto.DistanceUnit != null
                ? Microsoft.FSharp.Core.FSharpOption<DistanceUnit>.Some(
                    ToDistanceUnit(dto.DistanceUnit)
                )
                : Microsoft.FSharp.Core.FSharpOption<DistanceUnit>.None,
            dto.RideName != null
                ? Microsoft.FSharp.Core.FSharpOption<string>.Some(dto.RideName)
                : Microsoft.FSharp.Core.FSharpOption<string>.None,
            dto.StartLocation != null
                ? Microsoft.FSharp.Core.FSharpOption<string>.Some(dto.StartLocation)
                : Microsoft.FSharp.Core.FSharpOption<string>.None,
            dto.EndLocation != null
                ? Microsoft.FSharp.Core.FSharpOption<string>.Some(dto.EndLocation)
                : Microsoft.FSharp.Core.FSharpOption<string>.None,
            dto.Notes != null
                ? Microsoft.FSharp.Core.FSharpOption<string>.Some(dto.Notes)
                : Microsoft.FSharp.Core.FSharpOption<string>.None,
            dto.Latitude.HasValue
                ? Microsoft.FSharp.Core.FSharpOption<decimal>.Some(dto.Latitude.Value)
                : Microsoft.FSharp.Core.FSharpOption<decimal>.None,
            dto.Longitude.HasValue
                ? Microsoft.FSharp.Core.FSharpOption<decimal>.Some(dto.Longitude.Value)
                : Microsoft.FSharp.Core.FSharpOption<decimal>.None
        );
    }

    // ==================== Entity to DTO Mappings ====================

    public static RideResponse FromRide(Ride ride)
    {
        ArgumentNullException.ThrowIfNull(ride);
        return new RideResponse
        {
            RideId = ride.RideId,
            UserId = ride.UserId,
            Date = ride.Date,
            Hour = ride.Hour,
            Distance = ride.Distance,
            DistanceUnit = FromDistanceUnit(ride.DistanceUnit),
            RideName = ride.RideName,
            StartLocation = ride.StartLocation,
            EndLocation = ride.EndLocation,
            Notes = Microsoft.FSharp.Core.FSharpOption<string>.get_IsSome(ride.Notes)
                ? ride.Notes.Value
                : null,
            Weather = FromWeather(ride.WeatherData),
            CreatedAt = ride.CreatedTimestamp,
            ModifiedAt = Microsoft.FSharp.Core.FSharpOption<DateTime>.get_IsSome(
                ride.ModifiedTimestamp
            )
                ? ride.ModifiedTimestamp.Value
                : null,
            DeletionStatus = FromDeletionStatus(ride.DeletionStatus),
            CommunityStatus = FromCommunityStatus(ride.CommunityStatus),
            AgeInDays = ride.AgeInDays,
        };
    }

    public static RideListItemResponse FromRideProjection(
        BikeTracking.Domain.Entities.RideProjection projection
    )
    {
        ArgumentNullException.ThrowIfNull(projection);
        return new RideListItemResponse
        {
            RideId = projection.RideId,
            RideName = projection.RideName,
            StartLocation = projection.StartLocation,
            EndLocation = projection.EndLocation,
            Distance = projection.Distance,
            DistanceUnit = projection.DistanceUnit, // Already a string in C# projection
            AgeInDays = projection.AgeInDays,
            CreatedAt = projection.CreatedTimestamp,
            CanDelete =
                projection.AgeInDays <= 90
                && projection.DeletionStatus.Equals("active", StringComparison.OrdinalIgnoreCase),
        };
    }
}
