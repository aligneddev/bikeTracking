namespace BikeTracking.Tests.Domain.FSharp

open Xunit
open BikeTracking.Domain.FSharp.Results
open BikeTracking.Domain.FSharp.ValueObjects
open BikeTracking.Domain.FSharp.Entities
open System

module RideValidationTests =
    
    [<Fact>]
    let ``Ride validation succeeds for valid ride`` () =
        let today = DateOnly.FromDateTime(DateTime.UtcNow)
        let validRide = {
            RideId = Guid.NewGuid()
            UserId = "user-123"
            Date = today.AddDays(-10)
            Hour = 14
            Distance = 25.5m
            DistanceUnit = Miles
            RideName = "Morning Commute"
            StartLocation = "Home"
            EndLocation = "Office"
            Notes = Some "Great weather"
            WeatherData = None
            CreatedTimestamp = DateTime.UtcNow
            ModifiedTimestamp = None
            DeletionStatus = Active
            CommunityStatus = Private
        }
        
        match validRide.Validate() with
        | Success _ -> Assert.True(true)
        | Failure err -> Assert.True(false, $"Expected validation success but got: {err.Message}")
    
    [<Fact>]
    let ``Ride validation fails for future date`` () =
        let tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))
        let futureRide = {
            RideId = Guid.NewGuid()
            UserId = "user-123"
            Date = tomorrow
            Hour = 14
            Distance = 25.5m
            DistanceUnit = Miles
            RideName = "Future Ride"
            StartLocation = "Home"
            EndLocation = "Office"
            Notes = None
            WeatherData = None
            CreatedTimestamp = DateTime.UtcNow
            ModifiedTimestamp = None
            DeletionStatus = Active
            CommunityStatus = Private
        }
        
        match futureRide.Validate() with
        | Failure err -> 
            Assert.Equal("VALIDATION_FAILED", err.Code)
            Assert.Contains("future", err.Message.ToLower())
        | Success _ -> 
            Assert.True(false, "Expected validation failure for future date")
    
    [<Fact>]
    let ``Ride validation fails for date older than 90 days`` () =
        let oldDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-91))
        let oldRide = {
            RideId = Guid.NewGuid()
            UserId = "user-123"
            Date = oldDate
            Hour = 14
            Distance = 25.5m
            DistanceUnit = Miles
            RideName = "Old Ride"
            StartLocation = "Home"
            EndLocation = "Office"
            Notes = None
            WeatherData = None
            CreatedTimestamp = DateTime.UtcNow
            ModifiedTimestamp = None
            DeletionStatus = Active
            CommunityStatus = Private
        }
        
        match oldRide.Validate() with
        | Failure err -> 
            Assert.Equal("VALIDATION_FAILED", err.Code)
            Assert.Contains("90", err.Message)
        | Success _ -> 
            Assert.True(false, "Expected validation failure for old date")
    
    [<Fact>]
    let ``Ride validation fails for invalid hour`` () =
        let today = DateOnly.FromDateTime(DateTime.UtcNow)
        let invalidHourRide = {
            RideId = Guid.NewGuid()
            UserId = "user-123"
            Date = today
            Hour = 25
            Distance = 25.5m
            DistanceUnit = Miles
            RideName = "Invalid Hour"
            StartLocation = "Home"
            EndLocation = "Office"
            Notes = None
            WeatherData = None
            CreatedTimestamp = DateTime.UtcNow
            ModifiedTimestamp = None
            DeletionStatus = Active
            CommunityStatus = Private
        }
        
        match invalidHourRide.Validate() with
        | Failure err -> 
            Assert.Equal("VALIDATION_FAILED", err.Code)
            Assert.Contains("0 and 23", err.Message)
        | Success _ -> 
            Assert.True(false, "Expected validation failure for invalid hour")
    
    [<Fact>]
    let ``Ride validation fails for zero distance`` () =
        let today = DateOnly.FromDateTime(DateTime.UtcNow)
        let zeroDistanceRide = {
            RideId = Guid.NewGuid()
            UserId = "user-123"
            Date = today
            Hour = 14
            Distance = 0m
            DistanceUnit = Miles
            RideName = "Zero Distance"
            StartLocation = "Home"
            EndLocation = "Office"
            Notes = None
            WeatherData = None
            CreatedTimestamp = DateTime.UtcNow
            ModifiedTimestamp = None
            DeletionStatus = Active
            CommunityStatus = Private
        }
        
        match zeroDistanceRide.Validate() with
        | Failure err -> 
            Assert.Equal("VALIDATION_FAILED", err.Code)
            Assert.Contains("greater than zero", err.Message)
        | Success _ -> 
            Assert.True(false, "Expected validation failure for zero distance")
    
    [<Fact>]
    let ``Ride validation fails for empty ride name`` () =
        let today = DateOnly.FromDateTime(DateTime.UtcNow)
        let emptyNameRide = {
            RideId = Guid.NewGuid()
            UserId = "user-123"
            Date = today
            Hour = 14
            Distance = 10m
            DistanceUnit = Miles
            RideName = ""
            StartLocation = "Home"
            EndLocation = "Office"
            Notes = None
            WeatherData = None
            CreatedTimestamp = DateTime.UtcNow
            ModifiedTimestamp = None
            DeletionStatus = Active
            CommunityStatus = Private
        }
        
        match emptyNameRide.Validate() with
        | Failure err -> 
            Assert.Equal("VALIDATION_FAILED", err.Code)
            Assert.Contains("required", err.Message)
        | Success _ -> 
            Assert.True(false, "Expected validation failure for empty ride name")

module ValueObjectTests =
    
    [<Fact>]
    let ``DistanceUnit converts to string correctly`` () =
        Assert.Equal("miles", Miles.AsString())
        Assert.Equal("kilometers", Kilometers.AsString())
    
    [<Fact>]
    let ``DistanceUnit parses from string correctly`` () =
        match DistanceUnit.FromString("miles") with
        | Some Miles -> Assert.True(true)
        | _ -> Assert.True(false, "Expected Miles")
        
        match DistanceUnit.FromString("kilometers") with
        | Some Kilometers -> Assert.True(true)
        | _ -> Assert.True(false, "Expected Kilometers")
        
        match DistanceUnit.FromString("invalid") with
        | None -> Assert.True(true)
        | _ -> Assert.True(false, "Expected None for invalid input")
    
    [<Fact>]
    let ``Weather.IsUnavailable returns true for empty weather`` () =
        let unavailable = Weather.CreateUnavailable()
        Assert.True(unavailable.IsUnavailable)
    
    [<Fact>]
    let ``Weather.IsUnavailable returns false when data present`` () =
        let available = {
            Temperature = Some 72m
            Conditions = Some "Sunny"
            WindSpeed = None
            WindDirection = None
            Humidity = None
            Pressure = None
            CapturedAt = DateTime.UtcNow
        }
        Assert.False(available.IsUnavailable)

module ResultTests =
    
    [<Fact>]
    let ``Result.map transforms success value`` () =
        let result = Success 5
        let mapped = Result.map (fun x -> x * 2) result
        
        match mapped with
        | Success 10 -> Assert.True(true)
        | _ -> Assert.True(false, "Expected Success 10")
    
    [<Fact>]
    let ``Result.map preserves failure`` () =
        let error = Error.ValidationFailed "Test error"
        let result = Failure error
        let mapped = Result.map (fun x -> x * 2) result
        
        match mapped with
        | Failure err when err.Code = "VALIDATION_FAILED" -> Assert.True(true)
        | _ -> Assert.True(false, "Expected Failure with validation error")
    
    [<Fact>]
    let ``Result.bind chains successful operations`` () =
        let divide x y =
            if y = 0 then Failure (Error.ValidationFailed "Division by zero")
            else Success (x / y)
        
        let result = 
            Success 10
            |> Result.bind (fun x -> divide x 2)
            |> Result.bind (fun x -> divide x 5)
        
        match result with
        | Success 1 -> Assert.True(true)
        | _ -> Assert.True(false, "Expected Success 1")
    
    [<Fact>]
    let ``Result computation expression works`` () =
        let validateAge age =
            result {
                do! if age < 0 then 
                        Failure (Error.ValidationFailed "Age cannot be negative")
                    else Success ()
                
                do! if age > 150 then 
                        Failure (Error.ValidationFailed "Age too high")
                    else Success ()
                
                return age
            }
        
        match validateAge 25 with
        | Success 25 -> Assert.True(true)
        | _ -> Assert.True(false, "Expected Success 25")
        
        match validateAge -5 with
        | Failure _ -> Assert.True(true)
        | _ -> Assert.True(false, "Expected Failure for negative age")
