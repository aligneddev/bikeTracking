namespace BikeTracking.Domain.Entities;

/// <summary>
/// Pre-computed anonymous aggregate metrics for community features.
/// Updated daily with 0% PII exposure (SC-013, SC-014).
/// </summary>
public class CommunityStatistics
{
    public Guid StatisticId { get; set; }

    public int TotalRides { get; set; } // Count of all opted-in rides

    public decimal TotalDistance { get; set; } // Sum distance (normalized to km)

    public decimal AverageDistance { get; set; } // Mean distance per ride

    public string? RideFrequencyTrends { get; set; } // JSON: { "2025-01": 42, "2025-02": 38, ... }

    public string? LeaderboardData { get; set; } // JSON: [ { "anonymizedUserId": "...", "distance": 1234.5 }, ... ]

    public DateTime LastUpdated { get; set; }
}
