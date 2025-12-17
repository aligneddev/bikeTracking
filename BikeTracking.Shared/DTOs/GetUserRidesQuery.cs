namespace BikeTracking.Shared.DTOs;

/// <summary>
/// Query to get paginated user rides (T042).
/// </summary>
public class GetUserRidesQuery
{
    public required string UserId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}

