namespace BikeTracking.Domain.Entities;

/// <summary>
/// User-level settings and preferences.
/// Singleton per user (UserId is primary key).
/// </summary>
public class UserPreference
{
    public string UserId { get; set; } = null!; // OAuth identity, PK

    public string DistanceUnit { get; set; } = "miles"; // "miles" or "kilometers"

    public bool CommunityOptIn { get; set; }  // Consent for community features

    public DateTime CreatedTimestamp { get; set; }

    public DateTime? ModifiedTimestamp { get; set; }
}
