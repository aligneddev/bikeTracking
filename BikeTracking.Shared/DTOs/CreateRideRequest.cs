namespace BikeTracking.Shared.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Request DTO for creating a new ride (T026).
/// Includes DataAnnotations validation for Blazor forms and API validation.
/// </summary>
public class CreateRideRequest
{
    [Required(ErrorMessage = "Date is required")]
    public DateOnly Date { get; set; }

    [Range(0, 23, ErrorMessage = "Hour must be between 0 and 23")]
    public int Hour { get; set; }

    [Required(ErrorMessage = "Distance is required")]
    [Range(0.1, 10000, ErrorMessage = "Distance must be between 0.1 and 10000")]
    public decimal Distance { get; set; }

    [Required(ErrorMessage = "Distance unit is required")]
    [RegularExpression("^(miles|kilometers)$", ErrorMessage = "Distance unit must be miles or kilometers")]
    public string DistanceUnit { get; set; } = null!;

    [Required(ErrorMessage = "Ride name is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Ride name must be 1-200 characters")]
    public string RideName { get; set; } = null!;

    [Required(ErrorMessage = "Start location is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Start location must be 1-200 characters")]
    public string StartLocation { get; set; } = null!;

    [Required(ErrorMessage = "End location is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "End location must be 1-200 characters")]
    public string EndLocation { get; set; } = null!;

    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }

    /// <summary>
    /// Optional: latitude for weather API lookups (format: decimal)
    /// </summary>
    public decimal? Latitude { get; set; }

    /// <summary>
    /// Optional: longitude for weather API lookups (format: decimal)
    /// </summary>
    public decimal? Longitude { get; set; }
}

