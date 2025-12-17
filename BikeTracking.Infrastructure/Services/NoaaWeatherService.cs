using System.Net.Http.Json;

using BikeTracking.Domain.Services;
using BikeTracking.Domain.ValueObjects;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BikeTracking.Infrastructure.Services;

/// <summary>
/// NOAA Weather Service implementation with graceful degradation (FR-008).
/// Fetches historical hourly weather data from NOAA Climate Data Online API.
/// </summary>
public class NoaaWeatherService : IWeatherService
{
    private static readonly Action<ILogger, decimal, decimal, DateOnly, int, Exception?> _logFetchingWeather =
        LoggerMessage.Define<decimal, decimal, DateOnly, int>(
            logLevel: LogLevel.Information,
            eventId: new EventId(1, nameof(GetHistoricalWeatherAsync)),
            formatString: "Fetching weather for {Latitude},{Longitude} on {Date} at {Hour}:00");

    private static readonly Action<ILogger, object, decimal, decimal, Exception?> _logNoaaApiReturnedStatus =
        LoggerMessage.Define<object, decimal, decimal>(
            logLevel: LogLevel.Warning,
            eventId: new EventId(2, nameof(GetHistoricalWeatherAsync)),
            formatString: "NOAA API returned {StatusCode} for {Latitude},{Longitude}");

    private static readonly Action<ILogger, decimal, decimal, Exception?> _logNoaaApiNoPeriods =
        LoggerMessage.Define<decimal, decimal>(
            logLevel: LogLevel.Warning,
            eventId: new EventId(3, nameof(GetHistoricalWeatherAsync)),
            formatString: "NOAA API returned no periods for {Latitude},{Longitude}");

    private static readonly Action<ILogger, int, Exception?> _logNoWeatherDataForHour =
        LoggerMessage.Define<int>(
            logLevel: LogLevel.Warning,
            eventId: new EventId(4, nameof(GetHistoricalWeatherAsync)),
            formatString: "No weather data found for hour {Hour}");

    private static readonly Action<ILogger, Exception?> _logHttpError =
        LoggerMessage.Define(
            logLevel: LogLevel.Error,
            eventId: new EventId(5, nameof(GetHistoricalWeatherAsync)),
            formatString: "HTTP error fetching weather data");

    private static readonly Action<ILogger, Exception?> _logUnexpectedError =
        LoggerMessage.Define(
            logLevel: LogLevel.Error,
            eventId: new EventId(6, nameof(GetHistoricalWeatherAsync)),
            formatString: "Unexpected error fetching weather data");

    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<NoaaWeatherService> _logger;
    private readonly string _apiToken;
    private readonly string _baseUrl;

    public NoaaWeatherService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<NoaaWeatherService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        // API token is coming in the future
        //_apiToken = _configuration["WeatherService:ApiToken"] 
        //    ?? throw new InvalidOperationException("NOAA API token not configured");
        _baseUrl = _configuration["WeatherService:NoaaBaseUrl"]
            ?? "https://api.weather.gov";

        //_httpClient.DefaultRequestHeaders.Add("token", _apiToken);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "BikeTrackingDemo/1.0");
    }

    public async Task<Weather?> GetHistoricalWeatherAsync(
        decimal latitude,
        decimal longitude,
        DateOnly rideDate,
        int hour,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logFetchingWeather(_logger, latitude, longitude, rideDate, hour, null);

            // Construct request timestamp (NOAA requires ISO 8601 format)
            var requestDateTime = rideDate.ToDateTime(new TimeOnly(hour, 0));
            var endDateTime = requestDateTime.AddHours(1);

            // NOAA Weather API endpoint for point forecast
            var endpoint = $"{_baseUrl}/gridpoints/TOP/{latitude},{longitude}/forecast/hourly";

            var response = await _httpClient.GetAsync(endpoint, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logNoaaApiReturnedStatus(_logger, response.StatusCode, latitude, longitude, null);
                return CreateGracefulWeather();
            }

            var weatherResponse = await response.Content.ReadFromJsonAsync<NoaaWeatherResponse>(cancellationToken);

            if (weatherResponse?.Properties?.Periods == null || weatherResponse.Properties.Periods.Count == 0)
            {
                _logNoaaApiNoPeriods(_logger, latitude, longitude, null);
                return CreateGracefulWeather();
            }

            // Find the period matching our target hour
            var targetPeriod = weatherResponse.Properties.Periods
                .FirstOrDefault(p => DateTime.Parse(p.StartTime).Hour == hour);

            if (targetPeriod == null)
            {
                _logNoWeatherDataForHour(_logger, hour, null);
                return CreateGracefulWeather();
            }

            return new Weather
            {
                Temperature = targetPeriod.Temperature,
                Conditions = targetPeriod.ShortForecast,
                WindSpeed = ParseWindSpeed(targetPeriod.WindSpeed),
                WindDirection = targetPeriod.WindDirection,
                Humidity = targetPeriod.RelativeHumidity?.Value,
                Pressure = null, // NOAA hourly API doesn'\''t provide pressure
                CapturedAt = DateTime.UtcNow
            };
        }
        catch (HttpRequestException ex)
        {
            _logHttpError(_logger, ex);
            return CreateGracefulWeather();
        }
        catch (Exception ex)
        {
            _logUnexpectedError(_logger, ex);
            return CreateGracefulWeather();
        }
    }

    /// <summary>
    /// Creates a Weather object with all null values (graceful degradation per FR-008).
    /// </summary>
    private static Weather CreateGracefulWeather() => new Weather
    {
        Temperature = null,
        Conditions = null,
        WindSpeed = null,
        WindDirection = null,
        Humidity = null,
        Pressure = null,
        CapturedAt = DateTime.UtcNow
    };

    /// <summary>
    /// Parses wind speed from NOAA format (e.g., "10 mph") to decimal.
    /// </summary>
    private static decimal? ParseWindSpeed(string windSpeed)
    {
        if (string.IsNullOrEmpty(windSpeed))
            return null;

        var parts = windSpeed.Split(' ');
        if (parts.Length >= 1 && decimal.TryParse(parts[0], out var speed))
            return speed;

        return null;
    }
}

/// <summary>
/// NOAA Weather API response structure.
/// </summary>
internal sealed class NoaaWeatherResponse
{
    public NoaaProperties? Properties { get; set; }
}

internal sealed class NoaaProperties
{
    public List<NoaaPeriod> Periods { get; set; } = new();
}

internal sealed class NoaaPeriod
{
    public string StartTime { get; set; } = string.Empty;
    public decimal Temperature { get; set; }
    public string ShortForecast { get; set; } = string.Empty;
    public string WindSpeed { get; set; } = string.Empty;
    public string WindDirection { get; set; } = string.Empty;
    public NoaaValue? RelativeHumidity { get; set; }
}

internal sealed class NoaaValue
{
    public decimal? Value { get; set; }
}
