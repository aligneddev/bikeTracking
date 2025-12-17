namespace BikeTracking.Shared.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Request to edit an existing ride (T041).
/// All fields are optional - only provided fields are updated.
/// </summary>
public class EditRideRequest
{
    [Range(typeof(DateOnly), "2025-09-17", "2025-12-15", 
        ErrorMessage = "Date must be between 90 days ago and today")]
    public DateOnly? Date { get; set; }

    [Range(0, 23, ErrorMessage = "Hour must be between 0 and 23")]
    public int? Hour { get; set; }

    [Range(0.1, 10000, ErrorMessage = "Distance must be between 0.1 and 10,000")]
    public decimal? Distance { get; set; }

    [RegularExpression("miles|kilometers", 
        ErrorMessage = "DistanceUnit must be either miles or kilometers")]
    public string? DistanceUnit { get; set; }

    [StringLength(200, MinimumLength = 1, 
        ErrorMessage = "Ride name must be between 1 and 200 characters")]
    public string? RideName { get; set; }

    [StringLength(200, MinimumLength = 1, 
        ErrorMessage = "Start location must be between 1 and 200 characters")]
    public string? StartLocation { get; set; }

    [StringLength(200, MinimumLength = 1, 
        ErrorMessage = "End location must be between 1 and 200 characters")]
    public string? EndLocation { get; set; }

    [StringLength(1000, 
        ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }

    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
}

