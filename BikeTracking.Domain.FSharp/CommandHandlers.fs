namespace BikeTracking.Domain.FSharp

open System
open System.Threading
open System.Threading.Tasks
open BikeTracking.Domain.FSharp.Entities
open BikeTracking.Domain.FSharp.Events
open BikeTracking.Domain.FSharp.Results
open BikeTracking.Domain.FSharp.Services
open BikeTracking.Domain.FSharp.ValueObjects

module CommandHandlers =
    
    type CreateRideCommand = {
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
        Latitude: decimal option
        Longitude: decimal option
    }
    
    type CreateRideResult = {
        RideCreated: DomainEvent
        AdditionalEvents: DomainEvent list
    }
    
    type CreateRideCommandHandler(weatherService: IWeatherService) =
        
        member _.HandleAsync 
            (cmd: CreateRideCommand) 
            (cancellationToken: CancellationToken) 
            : Task<Result<CreateRideResult>> = 
            task {
                // Create ride entity
                let ride = {
                    RideId = cmd.RideId
                    UserId = cmd.UserId
                    Date = cmd.Date
                    Hour = cmd.Hour
                    Distance = cmd.Distance
                    DistanceUnit = cmd.DistanceUnit
                    RideName = cmd.RideName
                    StartLocation = cmd.StartLocation
                    EndLocation = cmd.EndLocation
                    Notes = cmd.Notes
                    WeatherData = None
                    CreatedTimestamp = DateTime.UtcNow
                    ModifiedTimestamp = None
                    DeletionStatus = Active
                    CommunityStatus = Private
                }
                
                // Validate
                match ride.Validate() with
                | Failure err -> return Failure err
                | Success _ ->
                    // Fetch weather if coordinates provided
                    let! weatherData, additionalEvents = 
                        match cmd.Latitude, cmd.Longitude with
                        | Some lat, Some lon ->
                            task {
                                try
                                    let! weather = 
                                        weatherService.GetHistoricalWeatherAsync lat lon cmd.Date cmd.Hour cancellationToken
                                    
                                    match weather with
                                    | Some w when not w.IsUnavailable ->
                                        let metadata = createMetadata cmd.RideId "Ride" cmd.UserId 1
                                        let weatherEvent = WeatherFetched {
                                            Metadata = metadata
                                            WeatherData = w
                                            SourceApi = "NOAA"
                                        }
                                        return (Some w, [weatherEvent])
                                    | _ -> 
                                        return (None, [])
                                with ex ->
                                    let metadata = createMetadata cmd.RideId "Ride" cmd.UserId 1
                                    let failEvent = WeatherFetchFailed {
                                        Metadata = metadata
                                        ErrorMessage = Some ex.Message
                                        SourceApi = "NOAA"
                                    }
                                    return (None, [failEvent])
                            }
                        | _ -> task { return (None, []) }
                    
                    // Create event
                    let metadata = createMetadata cmd.RideId "Ride" cmd.UserId 1
                    let rideCreatedEvent = RideCreated {
                        Metadata = metadata
                        Date = cmd.Date
                        Hour = cmd.Hour
                        Distance = cmd.Distance
                        DistanceUnit = cmd.DistanceUnit
                        RideName = cmd.RideName
                        StartLocation = cmd.StartLocation
                        EndLocation = cmd.EndLocation
                        Notes = cmd.Notes
                        WeatherData = weatherData
                    }
                    
                    return Success {
                        RideCreated = rideCreatedEvent
                        AdditionalEvents = additionalEvents
                    }
            }
    
    type EditRideCommand = {
        RideId: Guid
        UserId: string
        NewDate: DateOnly option
        NewHour: int option
        NewDistance: decimal option
        NewDistanceUnit: DistanceUnit option
        NewRideName: string option
        NewStartLocation: string option
        NewEndLocation: string option
        NewNotes: string option
        Latitude: decimal option
        Longitude: decimal option
    }
    
    type EditRideResult = {
        RideEdited: DomainEvent
        AdditionalEvents: DomainEvent list
    }
    
    type EditRideCommandHandler(weatherService: IWeatherService) =
        
        member _.HandleAsync 
            (cmd: EditRideCommand) 
            (currentRide: Ride)
            (cancellationToken: CancellationToken) 
            : Task<Result<EditRideResult>> = 
            task {
                // Apply changes to create updated ride
                let updatedRide = {
                    currentRide with
                        Date = cmd.NewDate |> Option.defaultValue currentRide.Date
                        Hour = cmd.NewHour |> Option.defaultValue currentRide.Hour
                        Distance = cmd.NewDistance |> Option.defaultValue currentRide.Distance
                        DistanceUnit = cmd.NewDistanceUnit |> Option.defaultValue currentRide.DistanceUnit
                        RideName = cmd.NewRideName |> Option.defaultValue currentRide.RideName
                        StartLocation = cmd.NewStartLocation |> Option.defaultValue currentRide.StartLocation
                        EndLocation = cmd.NewEndLocation |> Option.defaultValue currentRide.EndLocation
                        Notes = cmd.NewNotes |> Option.orElse currentRide.Notes
                        ModifiedTimestamp = Some DateTime.UtcNow
                }
                
                // Validate updated ride
                match updatedRide.Validate() with
                | Failure err -> return Failure err
                | Success _ ->
                    // Check if date/hour changed - need new weather
                    let dateTimeChanged = 
                        cmd.NewDate.IsSome || cmd.NewHour.IsSome
                    
                    let! newWeatherData, additionalEvents = 
                        if dateTimeChanged && cmd.Latitude.IsSome && cmd.Longitude.IsSome then
                            task {
                                try
                                    let! weather = 
                                        weatherService.GetHistoricalWeatherAsync 
                                            cmd.Latitude.Value cmd.Longitude.Value 
                                            updatedRide.Date updatedRide.Hour cancellationToken
                                    
                                    match weather with
                                    | Some w when not w.IsUnavailable ->
                                        let metadata = createMetadata cmd.RideId "Ride" cmd.UserId 2
                                        let weatherEvent = WeatherFetched {
                                            Metadata = metadata
                                            WeatherData = w
                                            SourceApi = "NOAA"
                                        }
                                        return (Some w, [weatherEvent])
                                    | _ -> 
                                        return (None, [])
                                with ex ->
                                    let metadata = createMetadata cmd.RideId "Ride" cmd.UserId 2
                                    let failEvent = WeatherFetchFailed {
                                        Metadata = metadata
                                        ErrorMessage = Some ex.Message
                                        SourceApi = "NOAA"
                                    }
                                    return (None, [failEvent])
                            }
                        else 
                            task { return (None, []) }
                    
                    // Build changed fields JSON (simplified)
                    let changedFields = 
                        [ cmd.NewDate |> Option.map (fun _ -> "Date")
                          cmd.NewHour |> Option.map (fun _ -> "Hour")
                          cmd.NewDistance |> Option.map (fun _ -> "Distance")
                          cmd.NewDistanceUnit |> Option.map (fun _ -> "DistanceUnit")
                          cmd.NewRideName |> Option.map (fun _ -> "RideName")
                          cmd.NewStartLocation |> Option.map (fun _ -> "StartLocation")
                          cmd.NewEndLocation |> Option.map (fun _ -> "EndLocation")
                          cmd.NewNotes |> Option.map (fun _ -> "Notes") ]
                        |> List.choose id
                        |> fun fields -> System.Text.Json.JsonSerializer.Serialize(fields)
                    
                    // Create event
                    let metadata = createMetadata cmd.RideId "Ride" cmd.UserId 2
                    let rideEditedEvent = RideEdited {
                        Metadata = metadata
                        ChangedFields = changedFields
                        NewDate = cmd.NewDate
                        NewHour = cmd.NewHour
                        NewDistance = cmd.NewDistance
                        NewRideName = cmd.NewRideName
                        NewStartLocation = cmd.NewStartLocation
                        NewEndLocation = cmd.NewEndLocation
                        NewNotes = cmd.NewNotes
                        NewWeatherData = newWeatherData
                    }
                    
                    return Success {
                        RideEdited = rideEditedEvent
                        AdditionalEvents = additionalEvents
                    }
            }
