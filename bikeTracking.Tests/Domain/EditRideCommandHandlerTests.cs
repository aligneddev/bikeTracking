using BikeTracking.Domain.Commands;
using BikeTracking.Domain.Entities;
using BikeTracking.Domain.Events;
using BikeTracking.Domain.Results;
using BikeTracking.Domain.Services;
using BikeTracking.Domain.ValueObjects;

using NSubstitute;

namespace bikeTracking.Tests.Domain;

/// <summary>
/// Unit tests for EditRideCommandHandler.
/// Tests orchestration logic for editing rides with mocked IWeatherService dependency.
/// Coverage: RideEdited event generation, weather re-fetching when date/hour changes,
/// change detection, validation error handling.
/// </summary>
[TestFixture]
public class EditRideCommandHandlerTests
{
    private IWeatherService _mockWeatherService = null!;
    private EditRideCommandHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _mockWeatherService = Substitute.For<IWeatherService>();
        _handler = new EditRideCommandHandler(_mockWeatherService);
    }

    private static RideProjection CreateTestRideProjection()
    {
        return new RideProjection
        {
            RideId = Guid.NewGuid(),
            UserId = "DEMO_User_TestUser",
            Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            Hour = 14,
            Distance = 10.0m,
            DistanceUnit = "miles",
            RideName = "DEMO_Original Ride",
            StartLocation = "DEMO_Home",
            EndLocation = "DEMO_Office",
            Notes = "DEMO_Original notes",
            CreatedTimestamp = DateTime.UtcNow.AddDays(-10),
            AgeInDays = 10
        };
    }

    #region Valid Edit Tests

    [Test]
    public async Task WhenEditingRideNameOnlyThenRideEditedEventGeneratedWithoutWeatherFetch()
    {
        // Arrange
        var currentRide = CreateTestRideProjection();
        var newRideName = "DEMO_Updated Ride Name";

        // Act
        var result = await _handler.HandleAsync(
            currentRide.RideId, currentRide.UserId, currentRide,
            null, null, null, null, newRideName, null, null, null, null, null);

        // Assert
        Assert.That(result, Is.InstanceOf<Result<(RideEdited, DomainEvent[])>.Success>());
        if (result is Result<(RideEdited, DomainEvent[])>.Success success)
        {
            var (rideEdited, additionalEvents) = success.Value;
            using (Assert.EnterMultipleScope())
            {
                Assert.That(rideEdited, Is.Not.Null);
                Assert.That(rideEdited.AggregateId, Is.EqualTo(currentRide.RideId));
                Assert.That(rideEdited.UserId, Is.EqualTo(currentRide.UserId));
                Assert.That(rideEdited.NewRideName, Is.EqualTo(newRideName));
                Assert.That(rideEdited.NewDate, Is.Null);
                Assert.That(rideEdited.NewHour, Is.Null);
                Assert.That(rideEdited.NewWeatherData, Is.Null);
                Assert.That(additionalEvents, Is.Empty);
            }
        }

        // Verify weather service was never called (date/hour unchanged)
        _ = await _mockWeatherService.DidNotReceive()
            .GetHistoricalWeatherAsync(Arg.Any<decimal>(), Arg.Any<decimal>(),
                Arg.Any<DateOnly>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task WhenEditingMultipleFieldsWithoutDateHourChangeThenNoWeatherFetch()
    {
        // Arrange
        var currentRide = CreateTestRideProjection();

        // Act
        var result = await _handler.HandleAsync(
            currentRide.RideId, currentRide.UserId, currentRide,
            null, null, 15.5m, "kilometers", "DEMO_New Name",
            "DEMO_New Start", "DEMO_New End", "DEMO_New notes", null, null);

        // Assert
        Assert.That(result, Is.InstanceOf<Result<(RideEdited, DomainEvent[])>.Success>());
        if (result is Result<(RideEdited, DomainEvent[])>.Success success)
        {
            var (rideEdited, additionalEvents) = success.Value;
            using (Assert.EnterMultipleScope())
            {
                Assert.That(rideEdited.NewDistance, Is.EqualTo(15.5m));
                Assert.That(rideEdited.NewRideName, Is.EqualTo("DEMO_New Name"));
                Assert.That(rideEdited.NewStartLocation, Is.EqualTo("DEMO_New Start"));
                Assert.That(rideEdited.NewEndLocation, Is.EqualTo("DEMO_New End"));
                Assert.That(rideEdited.NewNotes, Is.EqualTo("DEMO_New notes"));
                Assert.That(additionalEvents, Is.Empty);
            }
        }

        _ = await _mockWeatherService.DidNotReceive()
            .GetHistoricalWeatherAsync(Arg.Any<decimal>(), Arg.Any<decimal>(),
                Arg.Any<DateOnly>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region Weather Re-fetch Tests

    [Test]
    public async Task WhenDateChangesWithCoordinatesThenNewWeatherFetched()
    {
        // Arrange
        var currentRide = CreateTestRideProjection();
        var newDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5));
        var latitude = 37.7749m;
        var longitude = -122.4194m;

        var expectedWeather = new Weather
        {
            Temperature = 68.5m,
            Conditions = "Cloudy",
            WindSpeed = 8.0m,
            CapturedAt = DateTime.UtcNow
        };

        _ = _mockWeatherService.GetHistoricalWeatherAsync(
            latitude, longitude, newDate, currentRide.Hour, Arg.Any<CancellationToken>())
            .Returns(expectedWeather);

        // Act
        var result = await _handler.HandleAsync(
            currentRide.RideId, currentRide.UserId, currentRide,
            newDate, null, null, null, null, null, null, null, latitude, longitude);

        // Assert
        Assert.That(result, Is.InstanceOf<Result<(RideEdited, DomainEvent[])>.Success>());
        if (result is Result<(RideEdited, DomainEvent[])>.Success success)
        {
            var (rideEdited, additionalEvents) = success.Value;
            using (Assert.EnterMultipleScope())
            {
                Assert.That(rideEdited.NewDate, Is.EqualTo(newDate));
                Assert.That(rideEdited.NewWeatherData, Is.EqualTo(expectedWeather));
                Assert.That(additionalEvents, Has.Length.EqualTo(1));
                Assert.That(additionalEvents[0], Is.TypeOf<WeatherFetched>());

                var weatherEvent = (WeatherFetched)additionalEvents[0];
                Assert.That(weatherEvent.WeatherData, Is.EqualTo(expectedWeather));
            }
        }

        _ = await _mockWeatherService.Received(1)
            .GetHistoricalWeatherAsync(latitude, longitude, newDate, currentRide.Hour, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task WhenHourChangesWithCoordinatesThenNewWeatherFetched()
    {
        // Arrange
        var currentRide = CreateTestRideProjection();
        var newHour = 8;
        var latitude = 40.7128m;
        var longitude = -74.0060m;

        var expectedWeather = new Weather
        {
            Temperature = 55.0m,
            Conditions = "Rainy",
            WindSpeed = 12.5m,
            CapturedAt = DateTime.UtcNow
        };

        _ = _mockWeatherService.GetHistoricalWeatherAsync(
            latitude, longitude, currentRide.Date, newHour, Arg.Any<CancellationToken>())
            .Returns(expectedWeather);

        // Act
        var result = await _handler.HandleAsync(
            currentRide.RideId, currentRide.UserId, currentRide,
            null, newHour, null, null, null, null, null, null, latitude, longitude);

        // Assert
        Assert.That(result, Is.InstanceOf<Result<(RideEdited, DomainEvent[])>.Success>());
        if (result is Result<(RideEdited, DomainEvent[])>.Success success)
        {
            var (rideEdited, additionalEvents) = success.Value;
            using (Assert.EnterMultipleScope())
            {
                Assert.That(rideEdited.NewHour, Is.EqualTo(newHour));
                Assert.That(rideEdited.NewWeatherData, Is.EqualTo(expectedWeather));
                Assert.That(additionalEvents, Has.Length.EqualTo(1));
            }
        }

        _ = await _mockWeatherService.Received(1)
            .GetHistoricalWeatherAsync(latitude, longitude, currentRide.Date, newHour, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task WhenDateAndHourChangeThenNewWeatherFetched()
    {
        // Arrange
        var currentRide = CreateTestRideProjection();
        var newDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-3));
        var newHour = 18;
        var latitude = 34.0522m;
        var longitude = -118.2437m;

        var expectedWeather = new Weather
        {
            Temperature = 75.0m,
            Conditions = "Sunny",
            CapturedAt = DateTime.UtcNow
        };

        _ = _mockWeatherService.GetHistoricalWeatherAsync(
            latitude, longitude, newDate, newHour, Arg.Any<CancellationToken>())
            .Returns(expectedWeather);

        // Act
        var result = await _handler.HandleAsync(
            currentRide.RideId, currentRide.UserId, currentRide,
            newDate, newHour, null, null, null, null, null, null, latitude, longitude);

        // Assert
        Assert.That(result, Is.InstanceOf<Result<(RideEdited, DomainEvent[])>.Success>());
        if (result is Result<(RideEdited, DomainEvent[])>.Success success)
        {
            var (rideEdited, additionalEvents) = success.Value;
            using (Assert.EnterMultipleScope())
            {
                Assert.That(rideEdited.NewDate, Is.EqualTo(newDate));
                Assert.That(rideEdited.NewHour, Is.EqualTo(newHour));
                Assert.That(rideEdited.NewWeatherData, Is.EqualTo(expectedWeather));
            }
        }

        _ = await _mockWeatherService.Received(1)
            .GetHistoricalWeatherAsync(latitude, longitude, newDate, newHour, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task WhenDateChangesWithoutCoordinatesThenNoWeatherFetch()
    {
        // Arrange
        var currentRide = CreateTestRideProjection();
        var newDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2));

        // Act
        var result = await _handler.HandleAsync(
            currentRide.RideId, currentRide.UserId, currentRide,
            newDate, null, null, null, null, null, null, null, null, null);

        // Assert
        Assert.That(result, Is.InstanceOf<Result<(RideEdited, DomainEvent[])>.Success>());
        if (result is Result<(RideEdited, DomainEvent[])>.Success success)
        {
            var (rideEdited, additionalEvents) = success.Value;
            using (Assert.EnterMultipleScope())
            {
                Assert.That(rideEdited.NewDate, Is.EqualTo(newDate));
                Assert.That(rideEdited.NewWeatherData, Is.Null);
                Assert.That(additionalEvents, Is.Empty);
            }
        }

        _ = await _mockWeatherService.DidNotReceive()
            .GetHistoricalWeatherAsync(Arg.Any<decimal>(), Arg.Any<decimal>(),
                Arg.Any<DateOnly>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region Graceful Degradation Tests (FR-008)

    [Test]
    public async Task WhenWeatherServiceFailsDuringEditThenWeatherFetchFailedEventGenerated()
    {
        // Arrange
        var currentRide = CreateTestRideProjection();
        var newDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        var latitude = 51.5074m;
        var longitude = -0.1278m;

        _ = _mockWeatherService.GetHistoricalWeatherAsync(
            latitude, longitude, newDate, currentRide.Hour, Arg.Any<CancellationToken>())
            .Returns<Weather?>(_ => throw new TimeoutException("Weather API timeout"));

        // Act - Should not throw, graceful degradation
        var result = await _handler.HandleAsync(
            currentRide.RideId, currentRide.UserId, currentRide,
            newDate, null, null, null, null, null, null, null, latitude, longitude);

        // Assert - Result should be Success with WeatherFetchFailed event
        Assert.That(result, Is.InstanceOf<Result<(RideEdited, DomainEvent[])>.Success>());
        if (result is Result<(RideEdited, DomainEvent[])>.Success success)
        {
            var (rideEdited, additionalEvents) = success.Value;
            using (Assert.EnterMultipleScope())
            {
                Assert.That(rideEdited, Is.Not.Null);
                Assert.That(rideEdited.NewWeatherData, Is.Null);
                Assert.That(additionalEvents, Has.Length.EqualTo(1));
                Assert.That(additionalEvents[0], Is.TypeOf<WeatherFetchFailed>());

                var failedEvent = (WeatherFetchFailed)additionalEvents[0];
                Assert.That(failedEvent.ErrorMessage, Does.Contain("Weather API timeout"));
            }
        }
    }

    [Test]
    public async Task WhenWeatherServiceReturnsUnavailableDuringEditThenWeatherFetchFailedEventGenerated()
    {
        // Arrange
        var currentRide = CreateTestRideProjection();
        var newHour = 22;
        var latitude = 48.8566m;
        var longitude = 2.3522m;

        var unavailableWeather = Weather.CreateUnavailable();

        _ = _mockWeatherService.GetHistoricalWeatherAsync(
            latitude, longitude, currentRide.Date, newHour, Arg.Any<CancellationToken>())
            .Returns(unavailableWeather);

        // Act
        var result = await _handler.HandleAsync(
            currentRide.RideId, currentRide.UserId, currentRide,
            null, newHour, null, null, null, null, null, null, latitude, longitude);

        // Assert - Result should be Success with WeatherFetchFailed event
        Assert.That(result, Is.InstanceOf<Result<(RideEdited, DomainEvent[])>.Success>());
        if (result is Result<(RideEdited, DomainEvent[])>.Success success)
        {
            var (rideEdited, additionalEvents) = success.Value;
            using (Assert.EnterMultipleScope())
            {
                Assert.That(rideEdited.NewHour, Is.EqualTo(newHour));
                Assert.That(additionalEvents, Has.Length.EqualTo(1));
                Assert.That(additionalEvents[0], Is.TypeOf<WeatherFetchFailed>());

                var failedEvent = (WeatherFetchFailed)additionalEvents[0];
                Assert.That(failedEvent.ErrorMessage, Does.Contain("unavailable"));
            }
        }
    }

    #endregion

    #region Validation Tests

    [Test]
    public async Task WhenEditedRideBecomesInvalidThenReturnsValidationFailure()
    {
        // Arrange - Changing date to future violates validation
        var currentRide = CreateTestRideProjection();
        var futureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        // Act
        var result = await _handler.HandleAsync(
            currentRide.RideId, currentRide.UserId, currentRide,
            futureDate, null, null, null, null, null, null, null, null, null);

        // Assert
        Assert.That(result, Is.InstanceOf<Result<(RideEdited, DomainEvent[])>.Failure>());
        if (result is Result<(RideEdited, DomainEvent[])>.Failure failure)
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(failure.Error.Code, Is.EqualTo("VALIDATION_FAILED"));
                Assert.That(failure.Error.Message, Does.Contain("Date cannot be in the future"));
            }
        }
    }

    [Test]
    public async Task WhenCurrentRideIsNullThenReturnsValidationFailure()
    {
        // Arrange
        var rideId = Guid.NewGuid();
        var userId = "DEMO_User_TestUser";

        // Act
        var result = await _handler.HandleAsync(
            rideId, userId, null!,
            null, null, null, null, "DEMO_New Name", null, null, null, null, null);

        // Assert
        Assert.That(result, Is.InstanceOf<Result<(RideEdited, DomainEvent[])>.Failure>());
        if (result is Result<(RideEdited, DomainEvent[])>.Failure failure)
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(failure.Error.Code, Is.EqualTo("NOT_FOUND"));
                Assert.That(failure.Error.Message, Does.Contain("currentRide"));
            }
        }
    }

    #endregion
}
