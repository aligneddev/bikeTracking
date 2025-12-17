using BikeTracking.Domain.Commands;
using BikeTracking.Domain.Entities;
using BikeTracking.Domain.Events;
using BikeTracking.Domain.Services;
using BikeTracking.Domain.ValueObjects;

using NSubstitute;

namespace bikeTracking.Tests.Domain;

/// <summary>
/// Unit tests for CreateRideCommandHandler.
/// Tests orchestration logic with mocked IWeatherService dependency.
/// Coverage: RideCreated event generation, weather fetching with graceful degradation (FR-008),
/// validation error handling, event generation scenarios.
/// </summary>
[TestFixture]
public class CreateRideCommandHandlerTests
{
    private IWeatherService _mockWeatherService = null!;
    private CreateRideCommandHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _mockWeatherService = Substitute.For<IWeatherService>();
        _handler = new CreateRideCommandHandler(_mockWeatherService);
    }

    #region Valid Ride Creation Tests

    [Test]
    public async Task WhenValidRideWithoutCoordinatesThenRideCreatedEventGenerated()
    {
        // Arrange
        var rideId = Guid.NewGuid();
        var userId = "DEMO_User_TestUser";
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5));
        var hour = 14;
        var distance = 12.5m;
        var distanceUnit = "miles";
        var rideName = "DEMO_Afternoon Ride";
        var startLocation = "DEMO_Park";
        var endLocation = "DEMO_Office";
        var notes = "DEMO_Nice ride";

        // Act
        var (rideCreated, additionalEvents) = await _handler.HandleAsync(
            rideId, userId, date, hour, distance, distanceUnit,
            rideName, startLocation, endLocation, notes, null, null);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(rideCreated, Is.Not.Null);
            Assert.That(rideCreated.AggregateId, Is.EqualTo(rideId));
            Assert.That(rideCreated.UserId, Is.EqualTo(userId));
            Assert.That(rideCreated.Date, Is.EqualTo(date));
            Assert.That(rideCreated.Hour, Is.EqualTo(hour));
            Assert.That(rideCreated.Distance, Is.EqualTo(distance));
            Assert.That(rideCreated.DistanceUnit, Is.EqualTo(distanceUnit));
            Assert.That(rideCreated.RideName, Is.EqualTo(rideName));
            Assert.That(rideCreated.StartLocation, Is.EqualTo(startLocation));
            Assert.That(rideCreated.EndLocation, Is.EqualTo(endLocation));
            Assert.That(rideCreated.Notes, Is.EqualTo(notes));
            Assert.That(rideCreated.WeatherData, Is.Null);
            Assert.That(additionalEvents, Is.Empty);
        }

        // Verify weather service was never called
        await _mockWeatherService.DidNotReceive()
            .GetHistoricalWeatherAsync(Arg.Any<decimal>(), Arg.Any<decimal>(),
                Arg.Any<DateOnly>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task WhenWeatherServiceReturnsDataThenWeatherFetchedEventIsGenerated()
    {
        // Arrange
        var rideId = Guid.NewGuid();
        var userId = "DEMO_User_TestUser";
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-3));
        var hour = 10;
        var latitude = 37.7749m;
        var longitude = -122.4194m;

        var expectedWeather = new Weather
        {
            Temperature = 72.5m,
            Conditions = "Sunny",
            WindSpeed = 5.2m,
            WindDirection = "NW",
            Humidity = 65.0m,
            Pressure = 1013.25m,
            CapturedAt = DateTime.UtcNow
        };

        _mockWeatherService.GetHistoricalWeatherAsync(
            latitude, longitude, date, hour, Arg.Any<CancellationToken>())
            .Returns(expectedWeather);

        // Act
        var (rideCreated, additionalEvents) = await _handler.HandleAsync(
            rideId, userId, date, hour, 10.0m, "miles",
            "DEMO_Test Ride", "DEMO_Start", "DEMO_End", null, latitude, longitude);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(rideCreated.WeatherData, Is.EqualTo(expectedWeather));
            Assert.That(additionalEvents, Has.Length.EqualTo(1));
            Assert.That(additionalEvents[0], Is.TypeOf<WeatherFetched>());

            var weatherEvent = (WeatherFetched)additionalEvents[0];
            Assert.That(weatherEvent.AggregateId, Is.EqualTo(rideId));
            Assert.That(weatherEvent.UserId, Is.EqualTo(userId));
            Assert.That(weatherEvent.WeatherData, Is.EqualTo(expectedWeather));
            Assert.That(weatherEvent.SourceApi, Is.EqualTo("NOAA"));
        }

        await _mockWeatherService.Received(1)
            .GetHistoricalWeatherAsync(latitude, longitude, date, hour, Arg.Any<CancellationToken>());
    }

    #endregion

    #region Graceful Degradation Tests (FR-008)

    [Test]
    public async Task WhenWeatherServiceReturnsNullThenWeatherFetchFailedEventIsGenerated()
    {
        // Arrange
        var rideId = Guid.NewGuid();
        var userId = "DEMO_User_TestUser";
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2));
        var hour = 8;
        var latitude = 40.7128m;
        var longitude = -74.0060m;

        _mockWeatherService.GetHistoricalWeatherAsync(
            latitude, longitude, date, hour, Arg.Any<CancellationToken>())
            .Returns((Weather?)null);

        // Act
        var (rideCreated, additionalEvents) = await _handler.HandleAsync(
            rideId, userId, date, hour, 8.0m, "kilometers",
            "DEMO_Morning Commute", "DEMO_Home", "DEMO_Office", null, latitude, longitude);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(rideCreated.WeatherData, Is.Null);
            Assert.That(additionalEvents, Has.Length.EqualTo(1));
            Assert.That(additionalEvents[0], Is.TypeOf<WeatherFetchFailed>());

            var failedEvent = (WeatherFetchFailed)additionalEvents[0];
            Assert.That(failedEvent.AggregateId, Is.EqualTo(rideId));
            Assert.That(failedEvent.UserId, Is.EqualTo(userId));
            Assert.That(failedEvent.ErrorMessage, Does.Contain("unavailable"));
            Assert.That(failedEvent.SourceApi, Is.EqualTo("NOAA"));
        }
    }

    [Test]
    public async Task WhenWeatherServiceReturnsUnavailableDataThenWeatherFetchFailedEventIsGenerated()
    {
        // Arrange
        var rideId = Guid.NewGuid();
        var userId = "DEMO_User_TestUser";
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        var hour = 16;
        var latitude = 34.0522m;
        var longitude = -118.2437m;

        var unavailableWeather = Weather.CreateUnavailable();

        _mockWeatherService.GetHistoricalWeatherAsync(
            latitude, longitude, date, hour, Arg.Any<CancellationToken>())
            .Returns(unavailableWeather);

        // Act
        var (rideCreated, additionalEvents) = await _handler.HandleAsync(
            rideId, userId, date, hour, 15.0m, "miles",
            "DEMO_Evening Ride", "DEMO_Beach", "DEMO_Home", null, latitude, longitude);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(rideCreated, Is.Not.Null);
            Assert.That(additionalEvents, Has.Length.EqualTo(1));
            Assert.That(additionalEvents[0], Is.TypeOf<WeatherFetchFailed>());

            var failedEvent = (WeatherFetchFailed)additionalEvents[0];
            Assert.That(failedEvent.ErrorMessage, Does.Contain("unavailable"));
        }
    }

    [Test]
    public async Task WhenWeatherServiceThrowsExceptionThenWeatherFetchFailedEventIsGenerated()
    {
        // Arrange
        var rideId = Guid.NewGuid();
        var userId = "DEMO_User_TestUser";
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7));
        var hour = 12;
        var latitude = 51.5074m;
        var longitude = -0.1278m;

        _mockWeatherService.GetHistoricalWeatherAsync(
            latitude, longitude, date, hour, Arg.Any<CancellationToken>())
            .Returns<Weather?>(_ => throw new HttpRequestException("NOAA API timeout"));

        // Act
        var (rideCreated, additionalEvents) = await _handler.HandleAsync(
            rideId, userId, date, hour, 20.0m, "kilometers",
            "DEMO_Long Ride", "DEMO_City", "DEMO_Suburb", "DEMO_Windy day", latitude, longitude);

        // Assert - Ride creation should succeed despite weather failure
        using (Assert.EnterMultipleScope())
        {
            Assert.That(rideCreated, Is.Not.Null);
            Assert.That(rideCreated.WeatherData, Is.Null);
            Assert.That(additionalEvents, Has.Length.EqualTo(1));
            Assert.That(additionalEvents[0], Is.TypeOf<WeatherFetchFailed>());

            var failedEvent = (WeatherFetchFailed)additionalEvents[0];
            Assert.That(failedEvent.AggregateId, Is.EqualTo(rideId));
            Assert.That(failedEvent.ErrorMessage, Does.Contain("NOAA API timeout"));
        }
    }

    #endregion

    #region Validation Tests

    [Test]
    public void WhenInvalidRideDataThenThrowsInvalidOperationException()
    {
        // Arrange - Future date violates validation
        var rideId = Guid.NewGuid();
        var userId = "DEMO_User_TestUser";
        var futureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var hour = 14;

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _handler.HandleAsync(
                rideId, userId, futureDate, hour, 10.0m, "miles",
                "DEMO_Invalid Ride", "DEMO_Start", "DEMO_End", null, null, null));

        Assert.That(ex?.Message, Does.Contain("Ride validation failed"));
        Assert.That(ex?.Message, Does.Contain("Date cannot be in the future"));
    }

    [Test]
    public void WhenInvalidHourThenThrowsInvalidOperationException()
    {
        // Arrange - Hour > 23 violates validation
        var rideId = Guid.NewGuid();
        var userId = "DEMO_User_TestUser";
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5));
        var invalidHour = 25;

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _handler.HandleAsync(
                rideId, userId, date, invalidHour, 10.0m, "miles",
                "DEMO_Invalid Hour", "DEMO_Start", "DEMO_End", null, null, null));

        Assert.That(ex?.Message, Does.Contain("Ride validation failed"));
        Assert.That(ex?.Message, Does.Contain("Hour must be between 0 and 23"));
    }

    [Test]
    public void WhenEmptyRideNameThenThrowsInvalidOperationException()
    {
        // Arrange - Empty RideName violates validation
        var rideId = Guid.NewGuid();
        var userId = "DEMO_User_TestUser";
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2));
        var hour = 10;

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _handler.HandleAsync(
                rideId, userId, date, hour, 10.0m, "miles",
                "", "DEMO_Start", "DEMO_End", null, null, null));

        Assert.That(ex?.Message, Does.Contain("Ride validation failed"));
        Assert.That(ex?.Message, Does.Contain("Ride name is required"));
    }

    #endregion
}
