namespace BikeTracking.Domain.FSharp

open System
open BikeTracking.Domain.FSharp.ValueObjects

module Events =
    
    // Base event data that all events share
    type EventMetadata = {
        EventId: Guid
        AggregateId: Guid
        AggregateType: string
        UserId: string
        Timestamp: DateTime
        Version: int
    }
    
    type RideCreatedData = {
        Metadata: EventMetadata
        Date: DateOnly
        Hour: int
        Distance: decimal
        DistanceUnit: DistanceUnit
        RideName: string
        StartLocation: string
        EndLocation: string
        Notes: string option
        WeatherData: Weather option
    }
    
    type RideEditedData = {
        Metadata: EventMetadata
        ChangedFields: string  // JSON array
        NewDate: DateOnly option
        NewHour: int option
        NewDistance: decimal option
        NewRideName: string option
        NewStartLocation: string option
        NewEndLocation: string option
        NewNotes: string option
        NewWeatherData: Weather option
    }
    
    type DeletionType = 
        | Manual3Month
        | FormalRequest
        member this.AsString() =
            match this with
            | Manual3Month -> "manual_3m"
            | FormalRequest -> "formal_request"
    
    type RideDeletedData = {
        Metadata: EventMetadata
        DeletionType: DeletionType
    }
    
    type WeatherFetchedData = {
        Metadata: EventMetadata
        WeatherData: Weather
        SourceApi: string
    }
    
    type WeatherFetchFailedData = {
        Metadata: EventMetadata
        ErrorMessage: string option
        SourceApi: string
    }
    
    type DataDeletionScope =
        | OlderThan3Months
        | FullAccount
        member this.AsString() =
            match this with
            | OlderThan3Months -> "older_than_3_months"
            | FullAccount -> "full_account"
    
    type DataDeletionRequestedData = {
        Metadata: EventMetadata
        Scope: DataDeletionScope
        IdentityVerified: bool
    }
    
    type DataDeletionCompletedData = {
        Metadata: EventMetadata
        DeletedRideIds: Guid list option
        ProcessedTimestamp: DateTime
    }
    
    type CommunityOptInChangedData = {
        Metadata: EventMetadata
        OptInStatus: bool
    }
    
    type CommunityStatisticsUpdatedData = {
        Metadata: EventMetadata
        TotalRides: int
        TotalDistance: decimal
        AverageDistance: decimal
        UpdatedAt: DateTime
    }
    
    // Discriminated union of all domain events
    type DomainEvent =
        | RideCreated of RideCreatedData
        | RideEdited of RideEditedData
        | RideDeleted of RideDeletedData
        | WeatherFetched of WeatherFetchedData
        | WeatherFetchFailed of WeatherFetchFailedData
        | DataDeletionRequested of DataDeletionRequestedData
        | DataDeletionCompleted of DataDeletionCompletedData
        | CommunityOptInChanged of CommunityOptInChangedData
        | CommunityStatisticsUpdated of CommunityStatisticsUpdatedData
    with
        member this.EventId = 
            match this with
            | RideCreated e -> e.Metadata.EventId
            | RideEdited e -> e.Metadata.EventId
            | RideDeleted e -> e.Metadata.EventId
            | WeatherFetched e -> e.Metadata.EventId
            | WeatherFetchFailed e -> e.Metadata.EventId
            | DataDeletionRequested e -> e.Metadata.EventId
            | DataDeletionCompleted e -> e.Metadata.EventId
            | CommunityOptInChanged e -> e.Metadata.EventId
            | CommunityStatisticsUpdated e -> e.Metadata.EventId
        
        member this.AggregateId = 
            match this with
            | RideCreated e -> e.Metadata.AggregateId
            | RideEdited e -> e.Metadata.AggregateId
            | RideDeleted e -> e.Metadata.AggregateId
            | WeatherFetched e -> e.Metadata.AggregateId
            | WeatherFetchFailed e -> e.Metadata.AggregateId
            | DataDeletionRequested e -> e.Metadata.AggregateId
            | DataDeletionCompleted e -> e.Metadata.AggregateId
            | CommunityOptInChanged e -> e.Metadata.AggregateId
            | CommunityStatisticsUpdated e -> e.Metadata.AggregateId
        
        member this.UserId = 
            match this with
            | RideCreated e -> e.Metadata.UserId
            | RideEdited e -> e.Metadata.UserId
            | RideDeleted e -> e.Metadata.UserId
            | WeatherFetched e -> e.Metadata.UserId
            | WeatherFetchFailed e -> e.Metadata.UserId
            | DataDeletionRequested e -> e.Metadata.UserId
            | DataDeletionCompleted e -> e.Metadata.UserId
            | CommunityOptInChanged e -> e.Metadata.UserId
            | CommunityStatisticsUpdated e -> e.Metadata.UserId
        
        member this.Timestamp = 
            match this with
            | RideCreated e -> e.Metadata.Timestamp
            | RideEdited e -> e.Metadata.Timestamp
            | RideDeleted e -> e.Metadata.Timestamp
            | WeatherFetched e -> e.Metadata.Timestamp
            | WeatherFetchFailed e -> e.Metadata.Timestamp
            | DataDeletionRequested e -> e.Metadata.Timestamp
            | DataDeletionCompleted e -> e.Metadata.Timestamp
            | CommunityOptInChanged e -> e.Metadata.Timestamp
            | CommunityStatisticsUpdated e -> e.Metadata.Timestamp
        
        member this.Version = 
            match this with
            | RideCreated e -> e.Metadata.Version
            | RideEdited e -> e.Metadata.Version
            | RideDeleted e -> e.Metadata.Version
            | WeatherFetched e -> e.Metadata.Version
            | WeatherFetchFailed e -> e.Metadata.Version
            | DataDeletionRequested e -> e.Metadata.Version
            | DataDeletionCompleted e -> e.Metadata.Version
            | CommunityOptInChanged e -> e.Metadata.Version
            | CommunityStatisticsUpdated e -> e.Metadata.Version
        
        member this.EventType = 
            match this with
            | RideCreated _ -> "RideCreated"
            | RideEdited _ -> "RideEdited"
            | RideDeleted _ -> "RideDeleted"
            | WeatherFetched _ -> "WeatherFetched"
            | WeatherFetchFailed _ -> "WeatherFetchFailed"
            | DataDeletionRequested _ -> "DataDeletionRequested"
            | DataDeletionCompleted _ -> "DataDeletionCompleted"
            | CommunityOptInChanged _ -> "CommunityOptInChanged"
            | CommunityStatisticsUpdated _ -> "CommunityStatisticsUpdated"
    
    // Helper to create event metadata
    let createMetadata aggregateId aggregateType userId version = {
        EventId = Guid.NewGuid()
        AggregateId = aggregateId
        AggregateType = aggregateType
        UserId = userId
        Timestamp = DateTime.UtcNow
        Version = version
    }
