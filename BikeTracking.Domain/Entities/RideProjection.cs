namespace BikeTracking.Domain.Entities;

using BikeTracking.Domain.ValueObjects;

/// <summary>
/// Denormalized read model for ride queries.
/// Updated by event handlers when rides are created/edited/deleted.
/// Optimized for fast queries (list, search, filtering).
/// </summary>
public class RideProjection
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
    
    public Weather? WeatherData { get; set; }
    
    public DateTime CreatedTimestamp { get; set; }
    
    public DateTime? ModifiedTimestamp { get; set; }
    
    public string DeletionStatus { get; set; } = "active";
    
    public string CommunityStatus { get; set; } = "private";
    
    /// <summary>
    /// Computed column: days since creation (synced from database).
    /// </summary>
    public int AgeInDays { get; set; }
}
