using BikeTracking.Domain.Services;
using BikeTracking.Domain.ValueObjects;

namespace BikeTracking.Infrastructure.Services;

public class NoaaWeatherService : IWeatherService
{
    public Task<Weather?> GetHistoricalWeatherAsync(decimal latitude, decimal longitude, DateOnly date, int hour, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
