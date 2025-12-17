namespace BikeTracking.Domain.Entities;

/// <summary>
/// GDPR data deletion request tracking.
/// User-initiated deletion requests with 30-day processing window (FR-014, FR-015).
/// </summary>
public class DataDeletionRequest
{
    public Guid RequestId { get; set; }
    
    public string UserId { get; set; } = null!;
    
    public DateTime RequestTimestamp { get; set; }
    
    public string Status { get; set; } = "pending"; // "pending", "approved", "completed"
    
    public string Scope { get; set; } = null!; // "older_than_3_months" or "full_account"
    
    public DateTime? ProcessedTimestamp { get; set; }
    
    public string? AuditTrail { get; set; } // JSON: verification steps and actions taken
}
