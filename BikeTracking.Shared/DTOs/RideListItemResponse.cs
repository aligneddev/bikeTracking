namespace BikeTracking.Shared.DTOs;

/// <summary>
/// Summary view of a ride for list display (T043).
/// Lightweight response for paginated ride lists.
/// </summary>
public class RideListItemResponse
{
    public required Guid RideId { get; init; }
    public required string RideName { get; init; }
    public required string StartLocation { get; init; }
    public required string EndLocation { get; init; }
    public required decimal Distance { get; init; }
    public required string DistanceUnit { get; init; }
    public required int AgeInDays { get; init; }
    public bool CanDelete => AgeInDays <= 90;
    public required DateTime CreatedTimestamp { get; init; }
}

