namespace BikeTracking.Domain.Entities;

using BikeTracking.Domain.Results;
using BikeTracking.Domain.ValueObjects;

/// <summary>
/// Ride aggregate root - represents a single bike ride with weather conditions.
/// Enforces validation rules for dates (90-day window), hours (0-23), and required fields.
/// </summary>
public class Ride
{
    public Guid RideId { get; set; }

    public string UserId { get; set; } = null!;

    public DateOnly Date { get; set; }

    public int Hour { get; set; } // 0-23 (hourly granularity per spec)

    public decimal Distance { get; set; }

    public string DistanceUnit { get; set; } = "miles"; // "miles" or "kilometers"

    public string RideName { get; set; } = null!; // User-defined name, max 200 chars

    public string StartLocation { get; set; } = null!; // Max 200 chars

    public string EndLocation { get; set; } = null!; // Max 200 chars

    public string? Notes { get; set; }

    public Weather? WeatherData { get; set; }

    public DateTime CreatedTimestamp { get; set; }

    public DateTime? ModifiedTimestamp { get; set; }

    public string DeletionStatus { get; set; } = "active"; // "active" or "marked_for_deletion"

    public string CommunityStatus { get; set; } = "private"; // "private", "shareable", or "public"

    /// <summary>
    /// Computed column: days since creation.
    /// Used to enforce 90-day UI deletion window.
    /// </summary>
    public int AgeInDays => (int)(DateTime.UtcNow.Date - CreatedTimestamp.Date).TotalDays;

    /// <summary>
    /// Validates ride business rules using functional Result pattern.
    /// </summary>
    public Result<Unit> Validate()
    {
        // Date constraint: within last 90 days (FR-007, FR-011)
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var minDate = today.AddDays(-90);

        if (Date > today)
            return new Result<Unit>.Failure(
                Error.ValidationFailed("Date cannot be in the future."));

        if (Date < minDate)
            return new Result<Unit>.Failure(
                Error.ValidationFailed("Ride date must be within the last 90 days."));

        // Hour validation: 0-23 (FR-007)
        if (Hour < 0 || Hour > 23)
            return new Result<Unit>.Failure(
                Error.ValidationFailed("Hour must be between 0 and 23."));

        // Distance validation
        if (Distance <= 0)
            return new Result<Unit>.Failure(
                Error.ValidationFailed("Distance must be greater than zero."));

        // Unit validation
        if (DistanceUnit is not ("miles" or "kilometers"))
            return new Result<Unit>.Failure(
                Error.ValidationFailed("Distance unit must be 'miles' or 'kilometers'."));

        // Required fields
        if (string.IsNullOrWhiteSpace(RideName))
            return new Result<Unit>.Failure(
                Error.ValidationFailed("Ride name is required."));

        if (string.IsNullOrWhiteSpace(StartLocation))
            return new Result<Unit>.Failure(
                Error.ValidationFailed("Start location is required."));

        if (string.IsNullOrWhiteSpace(EndLocation))
            return new Result<Unit>.Failure(
                Error.ValidationFailed("End location is required."));

        // Character limits: 200 chars max (FR-001)
        if (RideName.Length > 200)
            return new Result<Unit>.Failure(
                Error.ValidationFailed("Ride name cannot exceed 200 characters."));

        if (StartLocation.Length > 200)
            return new Result<Unit>.Failure(
                Error.ValidationFailed("Start location cannot exceed 200 characters."));

        if (EndLocation.Length > 200)
            return new Result<Unit>.Failure(
                Error.ValidationFailed("End location cannot exceed 200 characters."));

        return new Result<Unit>.Success(Unit.Value);
    }
}
