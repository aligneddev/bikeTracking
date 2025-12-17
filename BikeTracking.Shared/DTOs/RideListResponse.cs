namespace BikeTracking.Shared.DTOs;

/// <summary>
/// Response DTO for paginated ride list (T049).
/// Wrapper for GET /api/rides endpoint response.
/// </summary>
public class RideListResponse
{
    public List<RideListItemResponse> Data { get; set; } = new();

    public int Total { get; set; }

    public int Page { get; set; }

    public int PageSize { get; set; }
}
