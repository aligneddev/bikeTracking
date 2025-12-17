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

        _apiToken = _configuration["WeatherService:ApiToken"] 
            ?? throw new InvalidOperationException("NOAA API token not configured");
        _baseUrl = _configuration["WeatherService:NoaaBaseUrl"] 
            ?? "https://api.weather.gov";

        _httpClient.DefaultRequestHeaders.Add("token", _apiToken);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "BikeTracking/1.0");
    }

    public async Task<Weather?> GetHistoricalWeatherAsync(
        decimal latitude,
        decimal longitude,
        DateOnly date,
        int hour,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Fetching weather for {Latitude},{Longitude} on {Date} at {Hour}:00",
                latitude, longitude, date, hour);

            // Construct request timestamp (NOAA requires ISO 8601 format)
            var requestDateTime = date.ToDateTime(new TimeOnly(hour, 0));
            var endDateTime = requestDateTime.AddHours(1);

            // NOAA Weather API endpoint for point forecast
            var endpoint = $"{_baseUrl}/gridpoints/TOP/{latitude},{longitude}/forecast/hourly";

            var response = await _httpClient.GetAsync(endpoint, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "NOAA API returned {StatusCode} for {Latitude},{Longitude}",
                    response.StatusCode, latitude, longitude);
                return CreateGracefulWeather();
            }

            var weatherResponse = await response.Content.ReadFromJsonAsync<NoaaWeatherResponse>(cancellationToken);

            if (weatherResponse?.Properties?.Periods == null || weatherResponse.Properties.Periods.Count == 0)
            {
                _logger.LogWarning("NOAA API returned no periods for {Latitude},{Longitude}", latitude, longitude);
                return CreateGracefulWeather();
            }

            // Find the period matching our target hour
            var targetPeriod = weatherResponse.Properties.Periods
                .FirstOrDefault(p => DateTime.Parse(p.StartTime).Hour == hour);

            if (targetPeriod == null)
            {
                _logger.LogWarning("No weather data found for hour {Hour}", hour);
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
            _logger.LogError(ex, "HTTP error fetching weather data");
            return CreateGracefulWeather();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching weather data");
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
internal class NoaaWeatherResponse
{
    public NoaaProperties? Properties { get; set; }
}

internal class NoaaProperties
{
    public List<NoaaPeriod> Periods { get; set; } = new();
}

internal class NoaaPeriod
{
    public string StartTime { get; set; } = string.Empty;
    public decimal Temperature { get; set; }
    public string ShortForecast { get; set; } = string.Empty;
    public string WindSpeed { get; set; } = string.Empty;
    public string WindDirection { get; set; } = string.Empty;
    public NoaaValue? RelativeHumidity { get; set; }
}

internal class NoaaValue
{
    public decimal? Value { get; set; }
}
