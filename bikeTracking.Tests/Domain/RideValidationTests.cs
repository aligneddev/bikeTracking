using BikeTracking.Domain.Entities;

namespace bikeTracking.Tests.Domain;

/// <summary>
/// Unit tests for Ride.IsValid() validation rules.
/// Tests pure business logic with no infrastructure dependencies.
/// Coverage: Date validation (90-day window), Hour validation (0-23),
/// Distance/Unit validation, Required fields, Character limits.
/// </summary>
[TestFixture]
public class RideValidationTests
{
    private static Ride CreateValidRide()
    {
        return new Ride
        {
            RideId = Guid.NewGuid(),
            UserId = "DEMO_User_TestUser",
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Hour = 12,
            Distance = 10.5m,
            DistanceUnit = "miles",
            RideName = "DEMO_Morning Commute",
            StartLocation = "DEMO_Home",
            EndLocation = "DEMO_Office",
            Notes = "DEMO_Test ride notes",
            CreatedTimestamp = DateTime.UtcNow
        };
    }

    #region Date Validation Tests

    [Test]
    public void WhenDateIsInFutureThenValidationFails()
    {
        // Arrange
        var ride = CreateValidRide();
        ride.Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        // Act
        var isValid = ride.IsValid(out var error);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(isValid, Is.False);
            Assert.That(error, Is.EqualTo("Date cannot be in the future."));
        }
    }

    [Test]
    public void WhenDateIsOlderThan90DaysThenValidationFails()
    {
        // Arrange
        var ride = CreateValidRide();
        ride.Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-91));

        // Act
        var isValid = ride.IsValid(out var error);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(isValid, Is.False);
            Assert.That(error, Is.EqualTo("Ride date must be within the last 90 days."));
        }
    }

    [Test]
    public void WhenDateIsTodayThenValidationSucceeds()
    {
        // Arrange
        var ride = CreateValidRide();
        ride.Date = DateOnly.FromDateTime(DateTime.UtcNow);

        // Act
        var isValid = ride.IsValid(out var error);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(isValid, Is.True);
            Assert.That(error, Is.Empty);
        }
    }

    [Test]
    public void WhenDateIsExactly90DaysAgoThenValidationSucceeds()
    {
        // Arrange
        var ride = CreateValidRide();
        ride.Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-90));

        // Act
        var isValid = ride.IsValid(out var error);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(isValid, Is.True);
            Assert.That(error, Is.Empty);
        }
    }

    [Test]
    public void WhenDateIsWithin90DayWindowThenValidationSucceeds()
    {
        // Arrange
        var ride = CreateValidRide();
        ride.Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-45)); // Middle of valid range

        // Act
        var isValid = ride.IsValid(out var error);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(isValid, Is.True);
            Assert.That(error, Is.Empty);
        }
    }

    #endregion

    #region Hour Validation Tests

    [Test]
    public void WhenHourIsNegativeThenValidationFails()
    {
        // Arrange
        var ride = CreateValidRide();
        ride.Hour = -1;

        // Act
        var isValid = ride.IsValid(out var error);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(isValid, Is.False);
            Assert.That(error, Is.EqualTo("Hour must be between 0 and 23."));
        }
    }

    [Test]
    public void WhenHourIsGreaterThan23ThenValidationFails()
    {
        // Arrange
        var ride = CreateValidRide();
        ride.Hour = 24;

        // Act
        var isValid = ride.IsValid(out var error);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(isValid, Is.False);
            Assert.That(error, Is.EqualTo("Hour must be between 0 and 23."));
        }
    }

    [Test]
    public void WhenHourIs0ThenValidationSucceeds()
    {
        // Arrange
        var ride = CreateValidRide();
        ride.Hour = 0;

        // Act
        var isValid = ride.IsValid(out var error);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(isValid, Is.True);
            Assert.That(error, Is.Empty);
        }
    }

    [Test]
    public void WhenHourIs23ThenValidationSucceeds()
    {
        // Arrange
        var ride = CreateValidRide();
        ride.Hour = 23;

        // Act
        var isValid = ride.IsValid(out var error);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(isValid, Is.True);
            Assert.That(error, Is.Empty);
        }
    }

    [Test]
    public void WhenHourIsValid12ThenValidationSucceeds()
    {
        // Arrange
        var ride = CreateValidRide();
        ride.Hour = 12;

        // Act
        var isValid = ride.IsValid(out var error);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(isValid, Is.True);
            Assert.That(error, Is.Empty);
        }
    }

    #endregion

    #region Distance Validation Tests

    [Test]
    public void WhenDistanceIsZeroThenValidationFails()
    {
        // Arrange
        var ride = CreateValidRide();
        ride.Distance = 0m;

        // Act
        var isValid = ride.IsValid(out var error);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(isValid, Is.False);
            Assert.That(error, Is.EqualTo("Distance must be greater than zero."));
        }
    }

    [Test]
    public void WhenDistanceIsNegativeThenValidationFails()
    {
        // Arrange
        var ride = CreateValidRide();
        ride.Distance = -5.5m;

        // Act
        var isValid = ride.IsValid(out var error);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(isValid, Is.False);
            Assert.That(error, Is.EqualTo("Distance must be greater than zero."));
        }
    }

    [Test]
    public void WhenDistanceIsPositiveThenValidationSucceeds()
    {
        // Arrange
        var ride = CreateValidRide();
        ride.Distance = 15.7m;

        // Act
        var isValid = ride.IsValid(out var error);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(isValid, Is.True);
            Assert.That(error, Is.Empty);
        }
    }

    #endregion

    #region Distance Unit Validation Tests

    [Test]
    public void WhenDistanceUnitIsMilesThenValidationSucceeds()
    {
        // Arrange
        var ride = CreateValidRide();
        ride.DistanceUnit = "miles";

        // Act
        var isValid = ride.IsValid(out var error);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(isValid, Is.True);
            Assert.That(error, Is.Empty);
        }
    }

    [Test]
    public void WhenDistanceUnitIsKilometersThenValidationSucceeds()
    {
        // Arrange
        var ride = CreateValidRide();
        ride.DistanceUnit = "kilometers";

        // Act
        var isValid = ride.IsValid(out var error);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(isValid, Is.True);
            Assert.That(error, Is.Empty);
        }
    }

    [Test]
    public void WhenDistanceUnitIsInvalidThenValidationFails()
    {
        // Arrange
        var ride = CreateValidRide();
        ride.DistanceUnit = "meters";

        // Act
        var isValid = ride.IsValid(out var error);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(isValid, Is.False);
            Assert.That(error, Is.EqualTo("Distance unit must be 'miles' or 'kilometers'."));
        }
    }

    [Test]
    public void WhenDistanceUnitIsEmptyThenValidationFails()
    {
        // Arrange
        var ride = CreateValidRide();
        ride.DistanceUnit = string.Empty;

        // Act
        var isValid = ride.IsValid(out var error);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(isValid, Is.False);
            Assert.That(error, Is.EqualTo("Distance unit must be 'miles' or 'kilometers'."));
        }
    }

    #endregion

    #region Required Fields Validation Tests

    [Test]
    public void WhenRideNameIsEmptyThenValidationFails()
    {
        // Arrange
        var ride = CreateValidRide();
        ride.RideName = string.Empty;

        // Act
        var isValid = ride.IsValid(out var error);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(isValid, Is.False);
            Assert.That(error, Is.EqualTo("Ride name is required."));
        }
    }

    [Test]
    public void WhenRideNameIsWhitespaceThenValidationFails()
    {
        // Arrange
        var ride = CreateValidRide();
        ride.RideName = "   ";

        // Act
        var isValid = ride.IsValid(out var error);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(isValid, Is.False);
            Assert.That(error, Is.EqualTo("Ride name is required."));
        }
    }

    [Test]
    public void WhenStartLocationIsEmptyThenValidationFails()
    {
        // Arrange
        var ride = CreateValidRide();
        ride.StartLocation = string.Empty;

        // Act
        var isValid = ride.IsValid(out var error);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(isValid, Is.False);
            Assert.That(error, Is.EqualTo("Start location is required."));
        }
    }

    [Test]
    public void WhenStartLocationIsWhitespaceThenValidationFails()
    {
        // Arrange
        var ride = CreateValidRide();
        ride.StartLocation = "   ";

        // Act
        var isValid = ride.IsValid(out var error);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(isValid, Is.False);
            Assert.That(error, Is.EqualTo("Start location is required."));
        }
    }

    [Test]
    public void WhenEndLocationIsEmptyThenValidationFails()
    {
        // Arrange
        var ride = CreateValidRide();
        ride.EndLocation = string.Empty;

        // Act
        var isValid = ride.IsValid(out var error);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(isValid, Is.False);
            Assert.That(error, Is.EqualTo("End location is required."));
        }
    }

    [Test]
    public void WhenEndLocationIsWhitespaceThenValidationFails()
    {
        // Arrange
        var ride = CreateValidRide();
        ride.EndLocation = "   ";

        // Act
        var isValid = ride.IsValid(out var error);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(isValid, Is.False);
            Assert.That(error, Is.EqualTo("End location is required."));
        }
    }

    #endregion

    #region Character Limit Tests

    [Test]
    public void WhenRideNameExceeds200CharsThenValidationFails()
    {
        // Arrange
        var ride = CreateValidRide();
        ride.RideName = new string('A', 201);

        // Act
        var isValid = ride.IsValid(out var error);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(isValid, Is.False);
            Assert.That(error, Is.EqualTo("Ride name cannot exceed 200 characters."));
        }
    }

    [Test]
    public void WhenRideNameIsExactly200CharsThenValidationSucceeds()
    {
        // Arrange
        var ride = CreateValidRide();
        ride.RideName = new string('A', 200);

        // Act
        var isValid = ride.IsValid(out var error);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(isValid, Is.True);
            Assert.That(error, Is.Empty);
        }
    }

    [Test]
    public void WhenStartLocationExceeds200CharsThenValidationFails()
    {
        // Arrange
        var ride = CreateValidRide();
        ride.StartLocation = new string('B', 201);

        // Act
        var isValid = ride.IsValid(out var error);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(isValid, Is.False);
            Assert.That(error, Is.EqualTo("Start location cannot exceed 200 characters."));
        }
    }

    [Test]
    public void WhenStartLocationIsExactly200CharsThenValidationSucceeds()
    {
        // Arrange
        var ride = CreateValidRide();
        ride.StartLocation = new string('B', 200);

        // Act
        var isValid = ride.IsValid(out var error);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(isValid, Is.True);
            Assert.That(error, Is.Empty);
        }
    }

    [Test]
    public void WhenEndLocationExceeds200CharsThenValidationFails()
    {
        // Arrange
        var ride = CreateValidRide();
        ride.EndLocation = new string('C', 201);

        // Act
        var isValid = ride.IsValid(out var error);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(isValid, Is.False);
            Assert.That(error, Is.EqualTo("End location cannot exceed 200 characters."));
        }
    }

    [Test]
    public void WhenEndLocationIsExactly200CharsThenValidationSucceeds()
    {
        // Arrange
        var ride = CreateValidRide();
        ride.EndLocation = new string('C', 200);

        // Act
        var isValid = ride.IsValid(out var error);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(isValid, Is.True);
            Assert.That(error, Is.Empty);
        }
    }

    #endregion

    #region Happy Path Test

    [Test]
    public void WhenAllFieldsAreValidThenValidationSucceeds()
    {
        // Arrange
        var ride = new Ride
        {
            RideId = Guid.NewGuid(),
            UserId = "DEMO_User_HappyPath",
            Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)),
            Hour = 8,
            Distance = 12.3m,
            DistanceUnit = "kilometers",
            RideName = "DEMO_Perfect Morning Ride",
            StartLocation = "DEMO_Downtown",
            EndLocation = "DEMO_Suburb",
            Notes = "DEMO_Beautiful weather, smooth ride",
            CreatedTimestamp = DateTime.UtcNow
        };

        // Act
        var isValid = ride.IsValid(out var error);

        // Assert
        using (Assert.EnterMultipleScope())
        {
            Assert.That(isValid, Is.True);
            Assert.That(error, Is.Empty);
        }
    }

    #endregion
}
