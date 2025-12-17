namespace BikeTracking.Shared.DTOs;


/// <summary>
/// Response DTO for ride details (T027).
/// Returned by GET /api/rides and POST /api/rides endpoints.
/// </summary>
public class RideResponse
{
    public Guid RideId { get; set; }

    public string UserId { get; set; } = null!;

    public DateOnly Date { get; set; }

    public int Hour { get; set; }

    public decimal Distance { get; set; }

    public string DistanceUnit { get; set; } = null!;

    public string RideName { get; set; } = null!;

    public string StartLocation { get; set; } = null!;

    public string EndLocation { get; set; } = null!;

    public string? Notes { get; set; }

    public WeatherResponse? Weather { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public string DeletionStatus { get; set; } = "active";

    public string CommunityStatus { get; set; } = "private";

    public int AgeInDays { get; set; }
}

/// <summary>
/// Weather data within ride response (T027).
/// Nullable fields support graceful degradation when API unavailable.
/// </summary>
public class WeatherResponse
{
    public decimal? Temperature { get; set; }

    public string? Conditions { get; set; }

    public decimal? WindSpeed { get; set; }

    public string? WindDirection { get; set; }

    public decimal? Humidity { get; set; }

    public decimal? Pressure { get; set; }

    public DateTime CapturedAt { get; set; }
}

/// <summary>
/// Summary view for ride list (used in pagination queries).
/// </summary>
public class RideListItemResponse
{
    public Guid RideId { get; set; }

    public string RideName { get; set; } = null!;

    public string StartLocation { get; set; } = null!;

    public string EndLocation { get; set; } = null!;

    public decimal Distance { get; set; }

    public string DistanceUnit { get; set; } = null!;

    public int AgeInDays { get; set; }

    public DateTime CreatedAt { get; set; }
}

