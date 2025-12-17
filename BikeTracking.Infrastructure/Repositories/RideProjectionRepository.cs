using BikeTracking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using BikeTracking.Infrastructure.Data;

namespace BikeTracking.Infrastructure.Repositories;
/// <summary>
/// Repository for ride projections (read model) (T029).
/// Handles queries for user rides with efficient indexing.
/// </summary>
public class RideProjectionRepository : IRideProjectionRepository
{
    private readonly BikeTrackingContext _context;

    public RideProjectionRepository(BikeTrackingContext context)
    {
        _context = context;
    }

    public async Task<RideProjection> CreateAsync(
        RideProjection projection,
        CancellationToken cancellationToken = default)
    {
        _context.RideProjections.Add(projection);
        await _context.SaveChangesAsync(cancellationToken);
        return projection;
    }

    public async Task<RideProjection?> GetByIdAsync(
        Guid rideId,
        CancellationToken cancellationToken = default)
    {
        return await _context.RideProjections
            .FirstOrDefaultAsync(r => r.RideId == rideId, cancellationToken);
    }

    public async Task<IEnumerable<RideProjection>> GetByUserIdAsync(
        string userId,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        return await _context.RideProjections
            .Where(r => r.UserId == userId && r.DeletionStatus == "active")
            .OrderByDescending(r => r.CreatedTimestamp)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<RideProjection> UpdateAsync(
        RideProjection projection,
        CancellationToken cancellationToken = default)
    {
        _context.RideProjections.Update(projection);
        await _context.SaveChangesAsync(cancellationToken);
        return projection;
    }

    public async Task DeleteAsync(
        Guid rideId,
        CancellationToken cancellationToken = default)
    {
        var projection = await _context.RideProjections
            .FirstOrDefaultAsync(r => r.RideId == rideId, cancellationToken);

        if (projection != null)
        {
            _context.RideProjections.Remove(projection);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<int> GetUserRideCountAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.RideProjections
            .CountAsync(r => r.UserId == userId && r.DeletionStatus == "active", cancellationToken);
    }
}

public interface IRideProjectionRepository
{
    Task<RideProjection> CreateAsync(RideProjection projection, CancellationToken cancellationToken = default);
    Task<RideProjection?> GetByIdAsync(Guid rideId, CancellationToken cancellationToken = default);
    Task<IEnumerable<RideProjection>> GetByUserIdAsync(string userId, int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default);
    Task<RideProjection> UpdateAsync(RideProjection projection, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid rideId, CancellationToken cancellationToken = default);
    Task<int> GetUserRideCountAsync(string userId, CancellationToken cancellationToken = default);
}

