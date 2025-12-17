using System.ComponentModel.DataAnnotations;

using BikeTracking.Shared.DTOs;

namespace bikeTracking.Tests.Domain;

/// <summary>
/// Unit tests for DataAnnotations validation on DTOs (Constitution Principle VII).
/// Tests ensure validation rules are enforced consistently across client and server.
/// Coverage: CreateRideRequest validation attributes, error messages, edge cases.
/// </summary>
[TestFixture]
public class DataValidationTests
{
    private static ValidationContext CreateValidationContext(object dto)
    {
        return new ValidationContext(dto, null, null);
    }

    private static List<ValidationResult> ValidateDto(object dto)
    {
        var validationResults = new List<ValidationResult>();
        var context = CreateValidationContext(dto);
        Validator.TryValidateObject(dto, context, validationResults, validateAllProperties: true);
        return validationResults;
    }

    #region CreateRideRequest Validation Tests

    [Test]
    public void WhenCreateRideRequestIsValidThenNoValidationErrors()
    {
        // Arrange
        var request = new CreateRideRequest
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Hour = 14,
            Distance = 12.5m,
            DistanceUnit = "miles",
            RideName = "DEMO_Valid Ride",
            StartLocation = "DEMO_Home",
            EndLocation = "DEMO_Office",
            Notes = "DEMO_Test notes",
            Latitude = 37.7749m,
            Longitude = -122.4194m
        };

        // Act
        var validationResults = ValidateDto(request);

        // Assert
        Assert.That(validationResults, Is.Empty);
    }

    [Test]
    public void WhenDistanceIsZeroThenValidationFails()
    {
        // Arrange
        var request = new CreateRideRequest
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Hour = 10,
            Distance = 0m, // Invalid: must be > 0.1
            DistanceUnit = "miles",
            RideName = "DEMO_Invalid Distance",
            StartLocation = "DEMO_Start",
            EndLocation = "DEMO_End"
        };

        // Act
        var validationResults = ValidateDto(request);

        // Assert
        Assert.That(validationResults, Has.Count.GreaterThan(0));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(validationResults.Any(v => v.MemberNames.Contains(nameof(CreateRideRequest.Distance))), Is.True);
            Assert.That(validationResults.First(v => v.MemberNames.Contains(nameof(CreateRideRequest.Distance))).ErrorMessage,
                Does.Contain("Distance must be between 0.1 and 10000"));
        }
    }

    [Test]
    public void WhenHourIsOutOfRangeThenValidationFails()
    {
        // Arrange
        var request = new CreateRideRequest
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Hour = 25, // Invalid: must be 0-23
            Distance = 10.0m,
            DistanceUnit = "miles",
            RideName = "DEMO_Invalid Hour",
            StartLocation = "DEMO_Start",
            EndLocation = "DEMO_End"
        };

        // Act
        var validationResults = ValidateDto(request);

        // Assert
        Assert.That(validationResults, Has.Count.GreaterThan(0));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(validationResults.Any(v => v.MemberNames.Contains(nameof(CreateRideRequest.Hour))), Is.True);
            Assert.That(validationResults.First(v => v.MemberNames.Contains(nameof(CreateRideRequest.Hour))).ErrorMessage,
                Does.Contain("Hour must be between 0 and 23"));
        }
    }

    [Test]
    public void WhenDistanceUnitIsInvalidThenValidationFails()
    {
        // Arrange
        var request = new CreateRideRequest
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Hour = 12,
            Distance = 10.0m,
            DistanceUnit = "meters", // Invalid: must be "miles" or "kilometers"
            RideName = "DEMO_Invalid Unit",
            StartLocation = "DEMO_Start",
            EndLocation = "DEMO_End"
        };

        // Act
        var validationResults = ValidateDto(request);

        // Assert
        Assert.That(validationResults, Has.Count.GreaterThan(0));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(validationResults.Any(v => v.MemberNames.Contains(nameof(CreateRideRequest.DistanceUnit))), Is.True);
            Assert.That(validationResults.First(v => v.MemberNames.Contains(nameof(CreateRideRequest.DistanceUnit))).ErrorMessage,
                Does.Contain("Distance unit must be miles or kilometers"));
        }
    }

    [Test]
    public void WhenRideNameIsEmptyThenValidationFails()
    {
        // Arrange
        var request = new CreateRideRequest
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Hour = 10,
            Distance = 10.0m,
            DistanceUnit = "miles",
            RideName = "", // Invalid: required, min length 1
            StartLocation = "DEMO_Start",
            EndLocation = "DEMO_End"
        };

        // Act
        var validationResults = ValidateDto(request);

        // Assert
        Assert.That(validationResults, Has.Count.GreaterThan(0));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(validationResults.Any(v => v.MemberNames.Contains(nameof(CreateRideRequest.RideName))), Is.True);
            Assert.That(validationResults.First(v => v.MemberNames.Contains(nameof(CreateRideRequest.RideName))).ErrorMessage,
                Does.Contain("Ride name"));
        }
    }

    [Test]
    public void WhenRideNameExceeds200CharsThenValidationFails()
    {
        // Arrange
        var request = new CreateRideRequest
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Hour = 10,
            Distance = 10.0m,
            DistanceUnit = "miles",
            RideName = new string('A', 201), // Invalid: max length 200
            StartLocation = "DEMO_Start",
            EndLocation = "DEMO_End"
        };

        // Act
        var validationResults = ValidateDto(request);

        // Assert
        Assert.That(validationResults, Has.Count.GreaterThan(0));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(validationResults.Any(v => v.MemberNames.Contains(nameof(CreateRideRequest.RideName))), Is.True);
            Assert.That(validationResults.First(v => v.MemberNames.Contains(nameof(CreateRideRequest.RideName))).ErrorMessage,
                Does.Contain("1-200 characters"));
        }
    }

    [Test]
    public void WhenStartLocationIsEmptyThenValidationFails()
    {
        // Arrange
        var request = new CreateRideRequest
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Hour = 10,
            Distance = 10.0m,
            DistanceUnit = "miles",
            RideName = "DEMO_Test Ride",
            StartLocation = "", // Invalid: required
            EndLocation = "DEMO_End"
        };

        // Act
        var validationResults = ValidateDto(request);

        // Assert
        Assert.That(validationResults, Has.Count.GreaterThan(0));
        Assert.That(validationResults.Any(v => v.MemberNames.Contains(nameof(CreateRideRequest.StartLocation))), Is.True);
    }

    [Test]
    public void WhenEndLocationIsEmptyThenValidationFails()
    {
        // Arrange
        var request = new CreateRideRequest
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Hour = 10,
            Distance = 10.0m,
            DistanceUnit = "miles",
            RideName = "DEMO_Test Ride",
            StartLocation = "DEMO_Start",
            EndLocation = "" // Invalid: required
        };

        // Act
        var validationResults = ValidateDto(request);

        // Assert
        Assert.That(validationResults, Has.Count.GreaterThan(0));
        Assert.That(validationResults.Any(v => v.MemberNames.Contains(nameof(CreateRideRequest.EndLocation))), Is.True);
    }

    [Test]
    public void WhenNotesExceed1000CharsThenValidationFails()
    {
        // Arrange
        var request = new CreateRideRequest
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Hour = 10,
            Distance = 10.0m,
            DistanceUnit = "miles",
            RideName = "DEMO_Test Ride",
            StartLocation = "DEMO_Start",
            EndLocation = "DEMO_End",
            Notes = new string('B', 1001) // Invalid: max length 1000
        };

        // Act
        var validationResults = ValidateDto(request);

        // Assert
        Assert.That(validationResults, Has.Count.GreaterThan(0));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(validationResults.Any(v => v.MemberNames.Contains(nameof(CreateRideRequest.Notes))), Is.True);
            Assert.That(validationResults.First(v => v.MemberNames.Contains(nameof(CreateRideRequest.Notes))).ErrorMessage,
                Does.Contain("1000 characters"));
        }
    }

    [Test]
    public void WhenNotesAreNullThenValidationSucceeds()
    {
        // Arrange - Notes are optional
        var request = new CreateRideRequest
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Hour = 10,
            Distance = 10.0m,
            DistanceUnit = "miles",
            RideName = "DEMO_Test Ride",
            StartLocation = "DEMO_Start",
            EndLocation = "DEMO_End",
            Notes = null // Optional field
        };

        // Act
        var validationResults = ValidateDto(request);

        // Assert
        Assert.That(validationResults, Is.Empty);
    }

    [Test]
    public void WhenLatitudeAndLongitudeAreNullThenValidationSucceeds()
    {
        // Arrange - Coordinates are optional
        var request = new CreateRideRequest
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Hour = 10,
            Distance = 10.0m,
            DistanceUnit = "kilometers",
            RideName = "DEMO_No Coordinates",
            StartLocation = "DEMO_Start",
            EndLocation = "DEMO_End",
            Latitude = null, // Optional
            Longitude = null // Optional
        };

        // Act
        var validationResults = ValidateDto(request);

        // Assert
        Assert.That(validationResults, Is.Empty);
    }

    [Test]
    public void WhenDistanceUnitIsMilesThenValidationSucceeds()
    {
        // Arrange
        var request = new CreateRideRequest
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Hour = 10,
            Distance = 10.0m,
            DistanceUnit = "miles", // Valid
            RideName = "DEMO_Miles Test",
            StartLocation = "DEMO_Start",
            EndLocation = "DEMO_End"
        };

        // Act
        var validationResults = ValidateDto(request);

        // Assert
        Assert.That(validationResults, Is.Empty);
    }

    [Test]
    public void WhenDistanceUnitIsKilometersThenValidationSucceeds()
    {
        // Arrange
        var request = new CreateRideRequest
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Hour = 10,
            Distance = 10.0m,
            DistanceUnit = "kilometers", // Valid
            RideName = "DEMO_Kilometers Test",
            StartLocation = "DEMO_Start",
            EndLocation = "DEMO_End"
        };

        // Act
        var validationResults = ValidateDto(request);

        // Assert
        Assert.That(validationResults, Is.Empty);
    }

    [Test]
    public void WhenMultipleFieldsAreInvalidThenAllValidationErrorsReturned()
    {
        // Arrange
        var request = new CreateRideRequest
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Hour = 25, // Invalid
            Distance = 0m, // Invalid
            DistanceUnit = "feet", // Invalid
            RideName = "", // Invalid
            StartLocation = "", // Invalid
            EndLocation = "" // Invalid
        };

        // Act
        var validationResults = ValidateDto(request);

        // Assert
        Assert.That(validationResults, Has.Count.GreaterThanOrEqualTo(5)); // At least 5 validation errors
    }

    #endregion

    #region Boundary Value Tests

    [Test]
    public void WhenDistanceIsMinimumValidValueThenValidationSucceeds()
    {
        // Arrange
        var request = new CreateRideRequest
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Hour = 10,
            Distance = 0.1m, // Minimum valid value
            DistanceUnit = "miles",
            RideName = "DEMO_Min Distance",
            StartLocation = "DEMO_Start",
            EndLocation = "DEMO_End"
        };

        // Act
        var validationResults = ValidateDto(request);

        // Assert
        Assert.That(validationResults, Is.Empty);
    }

    [Test]
    public void WhenDistanceIsMaximumValidValueThenValidationSucceeds()
    {
        // Arrange
        var request = new CreateRideRequest
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Hour = 10,
            Distance = 10000m, // Maximum valid value
            DistanceUnit = "miles",
            RideName = "DEMO_Max Distance",
            StartLocation = "DEMO_Start",
            EndLocation = "DEMO_End"
        };

        // Act
        var validationResults = ValidateDto(request);

        // Assert
        Assert.That(validationResults, Is.Empty);
    }

    [Test]
    public void WhenHourIs0ThenValidationSucceeds()
    {
        // Arrange
        var request = new CreateRideRequest
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Hour = 0, // Minimum valid value
            Distance = 10.0m,
            DistanceUnit = "miles",
            RideName = "DEMO_Midnight Ride",
            StartLocation = "DEMO_Start",
            EndLocation = "DEMO_End"
        };

        // Act
        var validationResults = ValidateDto(request);

        // Assert
        Assert.That(validationResults, Is.Empty);
    }

    [Test]
    public void WhenHourIs23ThenValidationSucceeds()
    {
        // Arrange
        var request = new CreateRideRequest
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Hour = 23, // Maximum valid value
            Distance = 10.0m,
            DistanceUnit = "miles",
            RideName = "DEMO_Late Night Ride",
            StartLocation = "DEMO_Start",
            EndLocation = "DEMO_End"
        };

        // Act
        var validationResults = ValidateDto(request);

        // Assert
        Assert.That(validationResults, Is.Empty);
    }

    #endregion
}
