namespace BikeTracking.Domain.FSharp

open System
open BikeTracking.Domain.FSharp.ValueObjects
open BikeTracking.Domain.FSharp.Results

module Entities =
    
    // Active patterns for validation
    let (|ValidDate|InvalidDate|) date =
        let today = DateOnly.FromDateTime(DateTime.UtcNow)
        let minDate = today.AddDays(-90)
        if date > today then InvalidDate "Date cannot be in the future."
        elif date < minDate then InvalidDate "Ride date must be within the last 90 days."
        else ValidDate

    let (|ValidHour|InvalidHour|) hour =
        if hour >= 0 && hour <= 23 then ValidHour
        else InvalidHour "Hour must be between 0 and 23."
    
    let (|ValidDistance|InvalidDistance|) distance =
        if distance <= 0m then InvalidDistance "Distance must be greater than zero."
        else ValidDistance
    
    let validateString maxLength fieldName (value: string) =
        if String.IsNullOrWhiteSpace(value) then 
            Failure (Error.ValidationFailed $"{fieldName} is required.")
        elif value.Length > maxLength then 
            Failure (Error.ValidationFailed $"{fieldName} cannot exceed {maxLength} characters.")
        else Success ()
    
    type Ride = {
        RideId: Guid
        UserId: string
        Date: DateOnly
        Hour: int
        Distance: decimal
        DistanceUnit: DistanceUnit
        RideName: string
        StartLocation: string
        EndLocation: string
        Notes: string option
        WeatherData: Weather option
        CreatedTimestamp: DateTime
        ModifiedTimestamp: DateTime option
        DeletionStatus: DeletionStatus
        CommunityStatus: CommunityStatus
    }
    with
        member this.AgeInDays = 
            (DateTime.UtcNow.Date - this.CreatedTimestamp.Date).Days
        
        member this.Validate() : Result<unit> =
            result {
                // Validate date
                match this.Date with
                | InvalidDate msg -> return! Failure (Error.ValidationFailed msg)
                | ValidDate -> ()
                
                // Validate hour
                match this.Hour with
                | InvalidHour msg -> return! Failure (Error.ValidationFailed msg)
                | ValidHour -> ()
                
                // Validate distance
                match this.Distance with
                | InvalidDistance msg -> return! Failure (Error.ValidationFailed msg)
                | ValidDistance -> ()
                
                // Validate ride name
                do! validateString 200 "Ride name" this.RideName
                
                // Validate start location
                do! validateString 200 "Start location" this.StartLocation
                
                // Validate end location
                do! validateString 200 "End location" this.EndLocation
                
                // Validate optional notes length
                match this.Notes with
                | Some notes when notes.Length > 1000 ->
                    return! Failure (Error.ValidationFailed "Notes cannot exceed 1000 characters.")
                | _ -> ()
                
                return ()
            }
    
    type RideProjection = {
        RideId: Guid
        UserId: string
        Date: DateOnly
        Hour: int
        Distance: decimal
        DistanceUnit: DistanceUnit
        RideName: string
        StartLocation: string
        EndLocation: string
        Notes: string option
        WeatherData: Weather option
        CreatedTimestamp: DateTime
        ModifiedTimestamp: DateTime option
        DeletionStatus: DeletionStatus
        CommunityStatus: CommunityStatus
        AgeInDays: int
    }
    
    type UserPreference = {
        UserId: string
        DistanceUnit: DistanceUnit
        CommunityOptIn: bool
        CreatedTimestamp: DateTime
        ModifiedTimestamp: DateTime option
    }
    
    type DataDeletionRequestStatus = 
        | Pending
        | Approved
        | Completed
        member this.AsString() =
            match this with
            | Pending -> "pending"
            | Approved -> "approved"
            | Completed -> "completed"
        static member FromString(s: string) =
            match s.ToLowerInvariant() with
            | "pending" -> Some Pending
            | "approved" -> Some Approved
            | "completed" -> Some Completed
            | _ -> None
    
    type DataDeletionRequest = {
        RequestId: Guid
        UserId: string
        RequestTimestamp: DateTime
        Status: DataDeletionRequestStatus
        Scope: Events.DataDeletionScope
        ProcessedTimestamp: DateTime option
        AuditTrail: string option
    }
    
    type CommunityStatistics = {
        StatisticId: Guid
        TotalRides: int
        TotalDistance: decimal
        AverageDistance: decimal
        RideFrequencyTrends: string option
        LeaderboardData: string option
        LastUpdated: DateTime
    }
