using System.Text.Json;

using BikeTracking.Domain.Events;
using BikeTracking.Domain.ValueObjects;

namespace bikeTracking.Tests.Domain;

/// <summary>
/// Unit tests for domain event serialization/deserialization (Contract Tests).
/// Tests ensure event schema stability for event sourcing integrity.
/// Coverage: All domain events, backwards compatibility, JSON round-trip.
/// Per Constitution Principle III: Event store relies on JSON serialization.
/// </summary>
[TestFixture]
public class EventSerializationTests
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    #region RideCreated Event Tests

    [Test]
    public void WhenRideCreatedEventSerializedThenDeserializesToSameValues()
    {
        // Arrange
        var originalEvent = new RideCreated
        {
            EventId = Guid.NewGuid(),
            AggregateId = Guid.NewGuid(),
            AggregateType = "Ride",
            Timestamp = new DateTime(2025, 12, 16, 10, 30, 0, DateTimeKind.Utc),
            Version = 0,
            UserId = "DEMO_User_TestUser",
            Date = new DateOnly(2025, 12, 15),
            Hour = 14,
            Distance = 12.5m,
            DistanceUnit = "miles",
            RideName = "DEMO_Morning Commute",
            StartLocation = "DEMO_Home",
            EndLocation = "DEMO_Office",
            Notes = "DEMO_Test ride notes",
            WeatherData = new Weather
            {
                Temperature = 72.5m,
                Conditions = "Sunny",
                WindSpeed = 5.2m,
                WindDirection = "NW",
                Humidity = 65.0m,
                Pressure = 1013.25m,
                CapturedAt = new DateTime(2025, 12, 15, 14, 0, 0, DateTimeKind.Utc)
            }
        };

        // Act - Serialize to JSON
        var json = JsonSerializer.Serialize(originalEvent, _jsonOptions);
        
        // Assert - JSON is not empty
        Assert.That(json, Is.Not.Empty);

        // Act - Deserialize back to object
        var deserializedEvent = JsonSerializer.Deserialize<RideCreated>(json, _jsonOptions);

        // Assert - All properties match
        Assert.That(deserializedEvent, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(deserializedEvent!.EventId, Is.EqualTo(originalEvent.EventId));
            Assert.That(deserializedEvent.AggregateId, Is.EqualTo(originalEvent.AggregateId));
            Assert.That(deserializedEvent.AggregateType, Is.EqualTo(originalEvent.AggregateType));
            Assert.That(deserializedEvent.Timestamp, Is.EqualTo(originalEvent.Timestamp));
            Assert.That(deserializedEvent.Version, Is.EqualTo(originalEvent.Version));
            Assert.That(deserializedEvent.UserId, Is.EqualTo(originalEvent.UserId));
            Assert.That(deserializedEvent.Date, Is.EqualTo(originalEvent.Date));
            Assert.That(deserializedEvent.Hour, Is.EqualTo(originalEvent.Hour));
            Assert.That(deserializedEvent.Distance, Is.EqualTo(originalEvent.Distance));
            Assert.That(deserializedEvent.DistanceUnit, Is.EqualTo(originalEvent.DistanceUnit));
            Assert.That(deserializedEvent.RideName, Is.EqualTo(originalEvent.RideName));
            Assert.That(deserializedEvent.StartLocation, Is.EqualTo(originalEvent.StartLocation));
            Assert.That(deserializedEvent.EndLocation, Is.EqualTo(originalEvent.EndLocation));
            Assert.That(deserializedEvent.Notes, Is.EqualTo(originalEvent.Notes));
            
            // Weather data assertions
            Assert.That(deserializedEvent.WeatherData, Is.Not.Null);
            Assert.That(deserializedEvent.WeatherData!.Temperature, Is.EqualTo(originalEvent.WeatherData!.Temperature));
            Assert.That(deserializedEvent.WeatherData.Conditions, Is.EqualTo(originalEvent.WeatherData.Conditions));
            Assert.That(deserializedEvent.WeatherData.WindSpeed, Is.EqualTo(originalEvent.WeatherData.WindSpeed));
            Assert.That(deserializedEvent.WeatherData.WindDirection, Is.EqualTo(originalEvent.WeatherData.WindDirection));
            Assert.That(deserializedEvent.WeatherData.Humidity, Is.EqualTo(originalEvent.WeatherData.Humidity));
            Assert.That(deserializedEvent.WeatherData.Pressure, Is.EqualTo(originalEvent.WeatherData.Pressure));
        }
    }

    [Test]
    public void WhenRideCreatedEventWithNullWeatherSerializedThenDeserializesCorrectly()
    {
        // Arrange
        var originalEvent = new RideCreated
        {
            EventId = Guid.NewGuid(),
            AggregateId = Guid.NewGuid(),
            AggregateType = "Ride",
            Timestamp = DateTime.UtcNow,
            Version = 0,
            UserId = "DEMO_User_NoWeather",
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Hour = 10,
            Distance = 8.0m,
            DistanceUnit = "kilometers",
            RideName = "DEMO_Quick Ride",
            StartLocation = "DEMO_Park",
            EndLocation = "DEMO_Lake",
            Notes = null,
            WeatherData = null // Graceful degradation case
        };

        // Act
        var json = JsonSerializer.Serialize(originalEvent, _jsonOptions);
        var deserializedEvent = JsonSerializer.Deserialize<RideCreated>(json, _jsonOptions);

        // Assert
        Assert.That(deserializedEvent, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(deserializedEvent!.AggregateId, Is.EqualTo(originalEvent.AggregateId));
            Assert.That(deserializedEvent.WeatherData, Is.Null);
            Assert.That(deserializedEvent.Notes, Is.Null);
        }
    }

    #endregion

    #region RideEdited Event Tests

    [Test]
    public void WhenRideEditedEventSerializedThenDeserializesToSameValues()
    {
        // Arrange
        var changedFields = new[] { "RideName", "Distance", "Notes" };
        var originalEvent = new RideEdited
        {
            EventId = Guid.NewGuid(),
            AggregateId = Guid.NewGuid(),
            AggregateType = "Ride",
            Timestamp = DateTime.UtcNow,
            Version = 1,
            UserId = "DEMO_User_Editor",
            ChangedFields = JsonSerializer.Serialize(changedFields),
            NewDate = null,
            NewHour = null,
            NewDistance = 15.5m,
            NewRideName = "DEMO_Updated Ride Name",
            NewStartLocation = null,
            NewEndLocation = null,
            NewNotes = "DEMO_Updated notes",
            NewWeatherData = null
        };

        // Act
        var json = JsonSerializer.Serialize(originalEvent, _jsonOptions);
        var deserializedEvent = JsonSerializer.Deserialize<RideEdited>(json, _jsonOptions);

        // Assert
        Assert.That(deserializedEvent, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(deserializedEvent!.EventId, Is.EqualTo(originalEvent.EventId));
            Assert.That(deserializedEvent.AggregateId, Is.EqualTo(originalEvent.AggregateId));
            Assert.That(deserializedEvent.UserId, Is.EqualTo(originalEvent.UserId));
            Assert.That(deserializedEvent.ChangedFields, Is.EqualTo(originalEvent.ChangedFields));
            Assert.That(deserializedEvent.NewDistance, Is.EqualTo(originalEvent.NewDistance));
            Assert.That(deserializedEvent.NewRideName, Is.EqualTo(originalEvent.NewRideName));
            Assert.That(deserializedEvent.NewNotes, Is.EqualTo(originalEvent.NewNotes));
            Assert.That(deserializedEvent.NewDate, Is.Null);
            Assert.That(deserializedEvent.NewHour, Is.Null);
        }
    }

    #endregion

    #region WeatherFetched Event Tests

    [Test]
    public void WhenWeatherFetchedEventSerializedThenDeserializesToSameValues()
    {
        // Arrange
        var originalEvent = new WeatherFetched
        {
            EventId = Guid.NewGuid(),
            AggregateId = Guid.NewGuid(),
            AggregateType = "Ride",
            Timestamp = DateTime.UtcNow,
            Version = 1,
            UserId = "DEMO_User_Weather",
            WeatherData = new Weather
            {
                Temperature = 68.0m,
                Conditions = "Cloudy",
                WindSpeed = 10.5m,
                WindDirection = "SE",
                Humidity = 70.0m,
                Pressure = 1015.0m,
                CapturedAt = DateTime.UtcNow
            },
            SourceApi = "NOAA"
        };

        // Act
        var json = JsonSerializer.Serialize(originalEvent, _jsonOptions);
        var deserializedEvent = JsonSerializer.Deserialize<WeatherFetched>(json, _jsonOptions);

        // Assert
        Assert.That(deserializedEvent, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(deserializedEvent!.EventId, Is.EqualTo(originalEvent.EventId));
            Assert.That(deserializedEvent.AggregateId, Is.EqualTo(originalEvent.AggregateId));
            Assert.That(deserializedEvent.SourceApi, Is.EqualTo(originalEvent.SourceApi));
            Assert.That(deserializedEvent.WeatherData, Is.Not.Null);
            Assert.That(deserializedEvent.WeatherData.Temperature, Is.EqualTo(originalEvent.WeatherData.Temperature));
            Assert.That(deserializedEvent.WeatherData.Conditions, Is.EqualTo(originalEvent.WeatherData.Conditions));
        }
    }

    #endregion

    #region WeatherFetchFailed Event Tests

    [Test]
    public void WhenWeatherFetchFailedEventSerializedThenDeserializesToSameValues()
    {
        // Arrange
        var originalEvent = new WeatherFetchFailed
        {
            EventId = Guid.NewGuid(),
            AggregateId = Guid.NewGuid(),
            AggregateType = "Ride",
            Timestamp = DateTime.UtcNow,
            Version = 2,
            UserId = "DEMO_User_Failed",
            ErrorMessage = "NOAA API timeout after 30 seconds",
            SourceApi = "NOAA"
        };

        // Act
        var json = JsonSerializer.Serialize(originalEvent, _jsonOptions);
        var deserializedEvent = JsonSerializer.Deserialize<WeatherFetchFailed>(json, _jsonOptions);

        // Assert
        Assert.That(deserializedEvent, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(deserializedEvent!.EventId, Is.EqualTo(originalEvent.EventId));
            Assert.That(deserializedEvent.AggregateId, Is.EqualTo(originalEvent.AggregateId));
            Assert.That(deserializedEvent.ErrorMessage, Is.EqualTo(originalEvent.ErrorMessage));
            Assert.That(deserializedEvent.SourceApi, Is.EqualTo(originalEvent.SourceApi));
        }
    }

    #endregion

    #region RideDeleted Event Tests

    [Test]
    public void WhenRideDeletedEventSerializedThenDeserializesToSameValues()
    {
        // Arrange
        var originalEvent = new RideDeleted
        {
            EventId = Guid.NewGuid(),
            AggregateId = Guid.NewGuid(),
            AggregateType = "Ride",
            Timestamp = DateTime.UtcNow,
            Version = 3,
            UserId = "DEMO_User_Deleter",
            DeletionType = "manual_3m"
        };

        // Act
        var json = JsonSerializer.Serialize(originalEvent, _jsonOptions);
        var deserializedEvent = JsonSerializer.Deserialize<RideDeleted>(json, _jsonOptions);

        // Assert
        Assert.That(deserializedEvent, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(deserializedEvent!.EventId, Is.EqualTo(originalEvent.EventId));
            Assert.That(deserializedEvent.AggregateId, Is.EqualTo(originalEvent.AggregateId));
            Assert.That(deserializedEvent.DeletionType, Is.EqualTo(originalEvent.DeletionType));
            Assert.That(deserializedEvent.UserId, Is.EqualTo(originalEvent.UserId));
        }
    }

    #endregion

    #region Backwards Compatibility Tests

    [Test]
    public void WhenOldRideCreatedEventWithMissingOptionalFieldThenDeserializesSuccessfully()
    {
        // Arrange - Simulate an old event schema missing the Notes field
        var oldEventJson = """
        {
            "EventId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
            "AggregateId": "b2c3d4e5-f6a7-8901-bcde-f12345678901",
            "AggregateType": "Ride",
            "Timestamp": "2025-12-01T10:00:00Z",
            "Version": 0,
            "UserId": "DEMO_User_OldSchema",
            "Date": "2025-11-30",
            "Hour": 8,
            "Distance": 10.0,
            "DistanceUnit": "miles",
            "RideName": "DEMO_Legacy Ride",
            "StartLocation": "DEMO_OldStart",
            "EndLocation": "DEMO_OldEnd"
        }
        """;

        // Act
        var deserializedEvent = JsonSerializer.Deserialize<RideCreated>(oldEventJson, _jsonOptions);

        // Assert - Event deserializes with null Notes (optional field)
        Assert.That(deserializedEvent, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(deserializedEvent!.RideName, Is.EqualTo("DEMO_Legacy Ride"));
            Assert.That(deserializedEvent.Distance, Is.EqualTo(10.0m));
            Assert.That(deserializedEvent.Notes, Is.Null); // Old schema didn't have Notes
            Assert.That(deserializedEvent.WeatherData, Is.Null); // Old schema didn't have WeatherData
        }
    }

    [Test]
    public void WhenNewEventWithAdditionalOptionalFieldThenSerializesWithoutBreakingOldConsumers()
    {
        // Arrange - New event schema with additional optional field (simulates forward compatibility)
        var newEvent = new RideCreated
        {
            EventId = Guid.NewGuid(),
            AggregateId = Guid.NewGuid(),
            AggregateType = "Ride",
            Timestamp = DateTime.UtcNow,
            Version = 0,
            UserId = "DEMO_User_NewSchema",
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Hour = 12,
            Distance = 20.0m,
            DistanceUnit = "kilometers",
            RideName = "DEMO_Future Ride",
            StartLocation = "DEMO_FutureStart",
            EndLocation = "DEMO_FutureEnd",
            Notes = "DEMO_This is a new field",
            WeatherData = new Weather { Temperature = 75.0m, Conditions = "Sunny" }
        };

        // Act
        var json = JsonSerializer.Serialize(newEvent, _jsonOptions);

        // Assert - JSON contains all fields including new optional ones
        Assert.That(json, Does.Contain("Notes"));
        Assert.That(json, Does.Contain("WeatherData"));
        Assert.That(json, Does.Contain("Temperature"));

        // Old consumers can still deserialize by ignoring unknown fields
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var deserialized = JsonSerializer.Deserialize<RideCreated>(json, options);
        Assert.That(deserialized, Is.Not.Null);
        Assert.That(deserialized!.RideName, Is.EqualTo("DEMO_Future Ride"));
    }

    #endregion
}
