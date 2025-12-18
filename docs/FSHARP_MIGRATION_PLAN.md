# F# Domain Migration Plan

## Executive Summary

This document outlines a comprehensive plan to convert the BikeTracking.Domain project from C# to F#. The migration will leverage F#'s strengths in domain modeling, particularly discriminated unions, type safety, and functional programming patterns.

## Current Domain Structure Analysis

### Project Overview
- **Target Framework**: .NET 10.0
- **Current Language**: C#
- **Dependencies**: None (pure domain layer)
- **Referenced By**: 
  - BikeTracking.Infrastructure
  - bikeTracking.ApiService
  - bikeTracking.Tests

### Domain Components

#### 1. **Entities** (5 files)
- `Ride.cs` - Aggregate root with validation logic
- `RideProjection.cs` - Read model for queries
- `UserPreference.cs` - User settings
- `DataDeletionRequest.cs` - GDPR tracking
- `CommunityStatistics.cs` - Anonymous aggregates

#### 2. **Value Objects** (1 file)
- `Weather.cs` - Immutable weather data

#### 3. **Results Pattern** (3 files)
- `Result.cs` - Discriminated union for success/failure
- `Error.cs` - Error representation with severity
- `ResultExtensions.cs` - Functional operations

#### 4. **Events** (10 files)
- `DomainEvent.cs` - Base class
- `RideCreated.cs`, `RideEdited.cs`, `RideDeleted.cs`
- `WeatherFetched.cs`, `WeatherFetchFailed.cs`
- `DataDeletionRequested.cs`, `DataDeletionCompleted.cs`
- `CommunityOptInChanged.cs`, `CommunityStatisticsUpdated.cs`

#### 5. **Commands** (2 files)
- `CreateRideCommandHandler.cs` - Command handler with async logic
- `EditRideCommandHandler.cs` - Update command handler

#### 6. **Services** (1 file)
- `IWeatherService.cs` - Interface for weather retrieval

---

## Migration Strategy

### Phase 1: Project Setup
1. Create new F# project `BikeTracking.Domain.FSharp`
2. Configure project:
   - Target Framework: net10.0
   - Enable nullable reference warnings
   - Add FSharp.Core package reference
3. Update solution references (maintain dual projects during transition)

### Phase 2: Core Types Migration (Priority Order)

#### 2.1 Value Objects & Basic Types
**Start here** - No dependencies, pure data structures

**Discriminated Union Opportunities:**
- ✅ **DistanceUnit**: `miles | kilometers` → Perfect DU candidate
- ✅ **DeletionStatus**: `active | marked_for_deletion` → DU
- ✅ **CommunityStatus**: `private | shareable | public` → DU
- ✅ **DataDeletionScope**: `older_than_3_months | full_account` → DU
- ✅ **DataDeletionStatus**: `pending | approved | completed` → DU
- ✅ **WindDirection**: N, NE, E, SE, S, SW, W, NW → DU with 8 cases

**Files to migrate:**
```fsharp
// ValueObjects.fs
module BikeTracking.Domain.ValueObjects

type DistanceUnit = 
    | Miles
    | Kilometers

type DeletionStatus = 
    | Active
    | MarkedForDeletion

type CommunityStatus = 
    | Private
    | Shareable  
    | Public

type WindDirection =
    | North | NorthEast | East | SouthEast
    | South | SouthWest | West | NorthWest

type Weather = {
    Temperature: decimal option
    Conditions: string option
    WindSpeed: decimal option
    WindDirection: WindDirection option
    Humidity: decimal option
    Pressure: decimal option
    CapturedAt: DateTime
}
with 
    member this.IsUnavailable = 
        this.Temperature.IsNone && this.Conditions.IsNone &&
        this.WindSpeed.IsNone && this.WindDirection.IsNone &&
        this.Humidity.IsNone && this.Pressure.IsNone
    
    static member CreateUnavailable() = {
        Temperature = None
        Conditions = None
        WindSpeed = None
        WindDirection = None
        Humidity = None
        Pressure = None
        CapturedAt = DateTime.UtcNow
    }
```

#### 2.2 Results & Error Handling
**Already partially using DUs!** Current C# uses records with inheritance.

**Discriminated Union Opportunities:**
- ✅ **Result<'T>**: Success | Failure → Already a DU in spirit
- ✅ **ErrorSeverity**: Warning | Error | Critical → DU

**Files to migrate:**
```fsharp
// Results.fs
module BikeTracking.Domain.Results

type ErrorSeverity = 
    | Warning
    | Error
    | Critical

type Error = {
    Code: string
    Message: string
    Severity: ErrorSeverity
}
with 
    static member ValidationFailed msg = 
        { Code = "VALIDATION_FAILED"; Message = msg; Severity = Warning }
    static member NotFound msg = 
        { Code = "NOT_FOUND"; Message = msg; Severity = Warning }
    static member Conflict msg = 
        { Code = "CONFLICT"; Message = msg; Severity = Warning }
    static member Unexpected msg = 
        { Code = "UNEXPECTED"; Message = msg; Severity = Error }
    static member Critical msg = 
        { Code = "CRITICAL"; Message = msg; Severity = Critical }
    static member Unauthorized msg = 
        { Code = "UNAUTHORIZED"; Message = msg; Severity = Warning }
    static member Forbidden msg = 
        { Code = "FORBIDDEN"; Message = msg; Severity = Warning }

type Result<'T> = 
    | Success of 'T
    | Failure of Error

module Result =
    let map f = function
        | Success value -> Success (f value)
        | Failure error -> Failure error
    
    let bind f = function
        | Success value -> f value
        | Failure error -> Failure error
    
    let tap f = function
        | Success value as result -> f value; result
        | other -> other
    
    let tapFailure f = function
        | Failure error as result -> f error; result
        | other -> other
    
    let getValueOrDefault defaultValue = function
        | Success value -> value
        | Failure _ -> defaultValue
    
    let getErrorOrNull = function
        | Failure error -> Some error
        | Success _ -> None

// Unit type for void operations
type Unit = unit
```

#### 2.3 Domain Events
**Discriminated Union Opportunities:**
- ✅ **DomainEvent**: Union of all event types → Excellent DU candidate!

**Files to migrate:**
```fsharp
// Events.fs
module BikeTracking.Domain.Events

open System
open BikeTracking.Domain.ValueObjects

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
        | RideCreated e -> e.EventId
        | RideEdited e -> e.EventId
        | RideDeleted e -> e.EventId
        | WeatherFetched e -> e.EventId
        | WeatherFetchFailed e -> e.EventId
        | DataDeletionRequested e -> e.EventId
        | DataDeletionCompleted e -> e.EventId
        | CommunityOptInChanged e -> e.EventId
        | CommunityStatisticsUpdated e -> e.EventId
    
    member this.AggregateId = 
        // Similar pattern for all cases...
        
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

and RideCreatedData = {
    EventId: Guid
    AggregateId: Guid
    UserId: string
    Timestamp: DateTime
    Version: int
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

// ... similar records for other event data types
```

#### 2.4 Entities
**Discriminated Union Opportunities:**
- Entity state validation errors can use DUs
- Computed properties can be functions

**Files to migrate:**
```fsharp
// Entities.fs
module BikeTracking.Domain.Entities

open System
open BikeTracking.Domain.ValueObjects
open BikeTracking.Domain.Results

type Ride = {
    RideId: Guid
    UserId: string
    Date: DateOnly
    Hour: int  // 0-23
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
        let today = DateOnly.FromDateTime(DateTime.UtcNow)
        let minDate = today.AddDays(-90)
        
        // Validation using computation expression (Railway Oriented Programming)
        result {
            do! if this.Date > today then 
                    Failure (Error.ValidationFailed "Date cannot be in the future.")
                else Success ()
            
            do! if this.Date < minDate then 
                    Failure (Error.ValidationFailed "Ride date must be within the last 90 days.")
                else Success ()
            
            do! if this.Hour < 0 || this.Hour > 23 then 
                    Failure (Error.ValidationFailed "Hour must be between 0 and 23.")
                else Success ()
            
            do! if this.Distance <= 0m then 
                    Failure (Error.ValidationFailed "Distance must be greater than zero.")
                else Success ()
            
            do! if String.IsNullOrWhiteSpace(this.RideName) then 
                    Failure (Error.ValidationFailed "Ride name is required.")
                else Success ()
            
            do! if this.RideName.Length > 200 then 
                    Failure (Error.ValidationFailed "Ride name cannot exceed 200 characters.")
                else Success ()
            
            // ... additional validations
            
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

type DataDeletionScope = 
    | OlderThan3Months
    | FullAccount

type DataDeletionRequest = {
    RequestId: Guid
    UserId: string
    RequestTimestamp: DateTime
    Status: DataDeletionRequestStatus
    Scope: DataDeletionScope
    ProcessedTimestamp: DateTime option
    AuditTrail: string option  // JSON
}

type CommunityStatistics = {
    StatisticId: Guid
    TotalRides: int
    TotalDistance: decimal
    AverageDistance: decimal
    RideFrequencyTrends: string option  // JSON
    LeaderboardData: string option      // JSON
    LastUpdated: DateTime
}
```

#### 2.5 Services Interface
```fsharp
// Services.fs
module BikeTracking.Domain.Services

open System
open System.Threading
open BikeTracking.Domain.ValueObjects

type IWeatherService =
    abstract GetHistoricalWeatherAsync: 
        latitude:decimal -> 
        longitude:decimal -> 
        rideDate:DateOnly -> 
        hour:int -> 
        cancellationToken:CancellationToken -> 
        Task<Weather option>
```

#### 2.6 Command Handlers
**Discriminated Union Opportunities:**
- Command input validation can use DUs
- Command results naturally fit Result<'T> pattern

```fsharp
// CommandHandlers.fs
module BikeTracking.Domain.Commands

open System
open System.Threading
open BikeTracking.Domain.Entities
open BikeTracking.Domain.Events
open BikeTracking.Domain.Results
open BikeTracking.Domain.Services
open BikeTracking.Domain.ValueObjects

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
                                let! weather = weatherService.GetHistoricalWeatherAsync(
                                    lat, lon, cmd.Date, cmd.Hour, cancellationToken)
                                
                                match weather with
                                | Some w when not w.IsUnavailable ->
                                    let weatherEvent = WeatherFetched {
                                        EventId = Guid.NewGuid()
                                        AggregateId = cmd.RideId
                                        UserId = cmd.UserId
                                        Timestamp = DateTime.UtcNow
                                        Version = 1
                                        WeatherData = w
                                        SourceApi = "NOAA"
                                    }
                                    return (Some w, [weatherEvent])
                                | _ -> 
                                    return (None, [])
                            with ex ->
                                let failEvent = WeatherFetchFailed {
                                    EventId = Guid.NewGuid()
                                    AggregateId = cmd.RideId
                                    UserId = cmd.UserId
                                    Timestamp = DateTime.UtcNow
                                    Version = 1
                                    Reason = ex.Message
                                }
                                return (None, [failEvent])
                        }
                    | _ -> task { return (None, []) }
                
                // Create event
                let rideCreatedEvent = RideCreated {
                    EventId = Guid.NewGuid()
                    AggregateId = cmd.RideId
                    UserId = cmd.UserId
                    Timestamp = DateTime.UtcNow
                    Version = 1
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
```

### Phase 3: Integration Changes

#### 3.1 Update BikeTracking.Infrastructure
- Update project reference to F# domain
- Adjust repository implementations for F# types
- Update EF Core mappings for discriminated unions
  - Use `.HasConversion()` for DU → string mapping
  - Example: `DeletionStatus.Active` ↔ `"active"`

```csharp
// BikeTrackingContext.cs updates needed
modelBuilder.Entity<Ride>()
    .Property(r => r.DistanceUnit)
    .HasConversion(
        v => v.ToString(),
        v => Enum.Parse<DistanceUnit>(v));
```

#### 3.2 Update bikeTracking.ApiService
- Update project reference
- Adjust endpoint handlers to work with F# Result types
- Map DUs to DTOs for API responses

```csharp
// Endpoints/RidesEndpoints.cs
app.MapPost("/api/rides", async (CreateRideDto dto, CreateRideCommandHandler handler) =>
{
    var result = await handler.HandleAsync(dto.ToCommand(), CancellationToken.None);
    
    return result switch
    {
        Result<CreateRideResult>.Success success => Results.Created($"/api/rides/{dto.RideId}", success.Value),
        Result<CreateRideResult>.Failure failure => Results.BadRequest(failure.Error),
        _ => Results.Problem("Unexpected result type")
    };
});
```

#### 3.3 Update Tests
- Update test project reference
- Adjust test assertions for F# types
- Use F# pattern matching in test helpers

```fsharp
// Domain/RideValidationTests.fs
[<Fact>]
let ``Ride validation fails for future date`` () =
    let futureRide = {
        // ... ride with future date
    }
    
    match futureRide.Validate() with
    | Failure err -> 
        Assert.Equal("VALIDATION_FAILED", err.Code)
        Assert.Contains("future", err.Message)
    | Success _ -> 
        Assert.True(false, "Expected validation failure")
```

### Phase 4: Advanced F# Features

#### 4.1 Computation Expressions
Implement custom computation expression for Result validation:

```fsharp
type ResultBuilder() =
    member _.Return(x) = Success x
    member _.ReturnFrom(x) = x
    member _.Bind(x, f) = Result.bind f x
    member _.Zero() = Success ()
    
let result = ResultBuilder()
```

#### 4.2 Active Patterns
Use active patterns for common validation scenarios:

```fsharp
let (|ValidDate|InvalidDate|) date =
    let today = DateOnly.FromDateTime(DateTime.UtcNow)
    let minDate = today.AddDays(-90)
    if date > today then InvalidDate "Date cannot be in the future"
    elif date < minDate then InvalidDate "Date must be within last 90 days"
    else ValidDate

let (|ValidHour|InvalidHour|) hour =
    if hour >= 0 && hour <= 23 then ValidHour
    else InvalidHour "Hour must be between 0 and 23"

// Usage in validation:
match ride.Date, ride.Hour with
| InvalidDate msg, _ -> Failure (Error.ValidationFailed msg)
| _, InvalidHour msg -> Failure (Error.ValidationFailed msg)
| ValidDate, ValidHour -> Success ()
```

#### 4.3 Type Providers (Future Enhancement)
Consider using type providers for:
- JSON schema validation (RideFrequencyTrends, LeaderboardData)
- SQL type checking at compile time

---

## Discriminated Union Summary

### Excellent DU Candidates (High Value)
1. ✅ **DistanceUnit** - 2 cases, clear semantics
2. ✅ **DeletionStatus** - 2 cases, state machine
3. ✅ **CommunityStatus** - 3 cases, access levels
4. ✅ **DataDeletionStatus** - 3 cases, workflow states
5. ✅ **DataDeletionScope** - 2 cases, clear distinction
6. ✅ **Result<'T>** - Already structured as DU in C#
7. ✅ **ErrorSeverity** - 3 cases, logging levels
8. ✅ **DomainEvent** - 9 cases, event sourcing (BIGGEST WIN!)
9. ✅ **WindDirection** - 8 cases, compass directions

### Benefits of DU Conversion
- **Type Safety**: Compile-time guarantees for exhaustive pattern matching
- **No Invalid States**: Impossible to create invalid combinations
- **Self-Documenting**: Union cases clearly show all possible states
- **Better Refactoring**: Compiler catches all locations when adding new cases
- **Performance**: DUs compiled to efficient IL, no inheritance overhead

---

## Migration Timeline

### Week 1: Foundation
- Day 1-2: Create F# project, configure build
- Day 3-4: Migrate ValueObjects and Results
- Day 5: Write unit tests for migrated components

### Week 2: Core Domain
- Day 1-2: Migrate Events with DU structure
- Day 3-4: Migrate Entities
- Day 5: Write domain validation tests

### Week 3: Commands & Integration
- Day 1-2: Migrate CommandHandlers
- Day 3-4: Update Infrastructure layer
- Day 5: Integration testing

### Week 4: Finalization
- Day 1-2: Update ApiService and endpoints
- Day 3-4: Full regression testing
- Day 5: Documentation and deployment

---

## Risk Mitigation

### Risks
1. **EF Core Integration**: F# records need special handling
   - **Mitigation**: Use mutable private setters or separate DB models
   
2. **Serialization**: DUs need custom JSON converters
   - **Mitigation**: Use FSharp.SystemTextJson package
   
3. **Async/Task Interop**: F# async vs C# Task
   - **Mitigation**: Use `task { }` computation expression (F# 6.0+)
   
4. **Learning Curve**: Team may be unfamiliar with F#
   - **Mitigation**: Code reviews, pair programming, F# training

### Rollback Strategy
- Maintain both C# and F# projects during transition
- Use feature flags to toggle between implementations
- Comprehensive test coverage before switchover

---

## Benefits of F# Migration

### Type Safety
- Discriminated unions eliminate invalid states
- Option types instead of null (no NullReferenceException)
- Pattern matching exhaustiveness checks

### Domain Modeling
- F# records are perfect for immutable entities
- Union types naturally represent business states
- Type-driven development enforces constraints

### Functional Patterns
- Result type for error handling (no exceptions)
- Composition over inheritance
- Pure functions for business logic

### Maintainability
- Less code: F# is ~30-40% more concise than C#
- Explicit state transitions with DUs
- Compile-time safety catches bugs early

### Performance
- Structural equality by default (records)
- Tail-call optimization for recursive functions
- DUs have minimal runtime overhead

---

## Appendix A: F# Project File

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <WarningLevel>5</WarningLevel>
    <OtherFlags>--warnon:1182</OtherFlags> <!-- Warn on unused variables -->
  </PropertyGroup>

  <ItemGroup>
    <Compile Include="Results.fs" />
    <Compile Include="ValueObjects.fs" />
    <Compile Include="Events.fs" />
    <Compile Include="Entities.fs" />
    <Compile Include="Services.fs" />
    <Compile Include="CommandHandlers.fs" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="FSharp.Core" Version="8.0.400" />
    <PackageReference Include="FSharp.SystemTextJson" Version="1.3.13" />
  </ItemGroup>

</Project>
```

---

## Appendix B: Blazor WASM & F# Serialization Strategy

### Challenge: F# DUs in Blazor WASM

Blazor WebAssembly uses `System.Text.Json` for all HTTP serialization. F# discriminated unions don't serialize well by default because:

1. **DU Structure**: F# DUs compile to complex class hierarchies
2. **Case Names**: Default serialization uses fully qualified type names
3. **HttpClient**: Blazor's `HttpClient.GetFromJsonAsync<T>()` uses System.Text.Json
4. **Size Matters**: WASM has strict size constraints - adding converters increases bundle size

### Recommended Solution: DTO Layer Pattern

**Keep F# domain pure, use C# DTOs for API boundary:**

```
┌─────────────────────┐      JSON/HTTP      ┌─────────────────────┐
│  Blazor WASM        │ ◄─────────────────► │   API Service       │
│  (C# DTOs)          │   System.Text.Json  │   (C# DTOs)         │
└─────────────────────┘                     └──────────┬──────────┘
                                                       │
                                                       │ Mapping
                                                       ▼
                                            ┌─────────────────────┐
                                            │   F# Domain         │
                                            │   (Pure DUs)        │
                                            └─────────────────────┘
```

**Current Architecture (Already Optimal!):**
- ✅ `BikeTracking.Shared/DTOs` - Plain C# classes for API contract
- ✅ `BikeTracking.Domain` - Will become F# with DUs
- ✅ API Service maps between DTOs and Domain

### Implementation Details

#### 1. Keep Existing DTOs (No Changes Needed)

```csharp
// BikeTracking.Shared/DTOs/RideResponse.cs
public class RideResponse
{
    public string DistanceUnit { get; set; } = null!;  // "miles" or "kilometers"
    public string DeletionStatus { get; set; } = "active";  // "active" or "marked_for_deletion"
    public string CommunityStatus { get; set; } = "private";  // "private", "shareable", "public"
    // ... other properties
}

// Blazor consumes these directly - no changes needed!
var rides = await Http.GetFromJsonAsync<List<RideResponse>>("/api/rides");
```

#### 2. Map DTOs to F# Domain in API Layer

```fsharp
// Mapping.fs (in API Service or Infrastructure)
module BikeTracking.Api.Mapping

open BikeTracking.Domain.ValueObjects
open BikeTracking.Shared.DTOs

let toDomainDistanceUnit (dto: string) : Result<DistanceUnit, string> =
    match dto.ToLowerInvariant() with
    | "miles" -> Ok Miles
    | "kilometers" -> Ok Kilometers
    | _ -> Error $"Invalid distance unit: {dto}"

let toDto (unit: DistanceUnit) : string =
    match unit with
    | Miles -> "miles"
    | Kilometers -> "kilometers"

let toDomainRide (dto: CreateRideRequest) : Result<CreateRideCommand, string> =
    result {
        let! distanceUnit = toDomainDistanceUnit dto.DistanceUnit
        return {
            RideId = Guid.NewGuid()
            UserId = "user-123"  // from auth context
            Date = dto.Date
            Hour = dto.Hour
            Distance = dto.Distance
            DistanceUnit = distanceUnit
            RideName = dto.RideName
            StartLocation = dto.StartLocation
            EndLocation = dto.EndLocation
            Notes = dto.Notes
            Latitude = dto.Latitude
            Longitude = dto.Longitude
        }
    }
```

#### 3. API Endpoint Mapping

```csharp
// bikeTracking.ApiService/Endpoints/RidesEndpoints.cs
app.MapPost("/api/rides", async (
    CreateRideRequest dto,
    CreateRideCommandHandler handler,
    IMapper mapper) =>
{
    // DTO → F# Domain
    var commandResult = mapper.ToCreateRideCommand(dto);
    if (commandResult.IsFailure)
        return Results.BadRequest(commandResult.Error);
    
    // Execute F# command handler
    var result = await handler.HandleAsync(commandResult.Value, CancellationToken.None);
    
    // F# Domain → DTO
    return result switch
    {
        Result<CreateRideResult>.Success success => 
            Results.Created($"/api/rides/{success.Value.RideId}", 
                           mapper.ToRideResponse(success.Value)),
        Result<CreateRideResult>.Failure failure => 
            Results.BadRequest(new { error = failure.Error.Message }),
        _ => Results.Problem("Unexpected result")
    };
});
```

### Alternative: Use FSharp.SystemTextJson (More Complex)

If you want to use F# types directly in Blazor WASM:

**Pros:**
- Single type system across all layers
- No mapping code needed

**Cons:**
- ❌ Increases WASM bundle size (~500KB for FSharp.Core + converters)
- ❌ More complex serialization setup
- ❌ Potential runtime errors if converters misconfigured
- ❌ Blazor form validation harder with F# types

**Setup (if choosing this path):**

```xml
<!-- bikeTracking.WebWasm.csproj -->
<ItemGroup>
  <PackageReference Include="FSharp.SystemTextJson" Version="1.3.13" />
  <ProjectReference Include="..\BikeTracking.Domain.FSharp\BikeTracking.Domain.FSharp.fsproj" />
</ItemGroup>
```

```csharp
// Program.cs
using System.Text.Json;
using System.Text.Json.Serialization;

builder.Services.AddScoped(sp => 
{
    var options = new JsonSerializerOptions
    {
        Converters = { 
            new JsonFSharpConverter(
                JsonUnionEncoding.AdjacentTag |  // {"Case":"Miles"}
                JsonUnionFlags.UnwrapOption |    // null for None
                JsonUnionFlags.UnwrapSingleCaseUnions
            )
        }
    };
    
    var client = new HttpClient { BaseAddress = new Uri("...") };
    // Custom JsonSerializerOptions for HttpClient
    return client;
});
```

### Recommended Approach: Hybrid Strategy

**For String-Based Enums (DistanceUnit, DeletionStatus, etc.):**
- Keep as strings in DTOs (simple, WASM-friendly)
- Map to F# DUs in API layer only

**For Complex Domain Logic (Result, DomainEvent):**
- F# types stay server-side only
- Never sent to Blazor WASM
- DTOs represent API contract

**Benefits:**
- ✅ Blazor WASM stays lightweight
- ✅ No F# runtime in browser
- ✅ Standard Blazor patterns work
- ✅ F# domain remains pure
- ✅ Clear separation of concerns

### Updated Migration Plan

**Phase 3.2 (Revised): Update bikeTracking.ApiService**

1. Create mapping layer between DTOs and F# domain:
   ```
   bikeTracking.ApiService/
   ├── Endpoints/
   │   └── RidesEndpoints.cs  (mapping logic here)
   └── Mapping/
       ├── DomainToDtoMapper.cs
       └── DtoToDomainMapper.fs  (F# module)
   ```

2. Update endpoints to use mappers
3. Keep `BikeTracking.Shared/DTOs` unchanged
4. Blazor WASM needs zero changes!

**Testing Strategy:**

```csharp
// Integration test
[Fact]
public async Task BlazorClient_CanDeserialize_RideResponse()
{
    var json = """{"rideId":"...", "distanceUnit":"miles", ...}""";
    var options = new JsonSerializerOptions();
    
    var ride = JsonSerializer.Deserialize<RideResponse>(json, options);
    
    Assert.NotNull(ride);
    Assert.Equal("miles", ride.DistanceUnit);
}
```

---

## Appendix C: JSON Serialization Setup (Server-Side Only)

**F# domain types are server-side only - no client serialization needed!**

```fsharp
// Only needed in Infrastructure layer for event store serialization
// JsonConverters.fs
open System.Text.Json
open System.Text.Json.Serialization

type DistanceUnitConverter() =
    inherit JsonConverter<DistanceUnit>()
    
    override _.Read(reader, typeToConvert, options) =
        match reader.GetString() with
        | "miles" -> Miles
        | "kilometers" -> Kilometers
        | s -> failwithf "Unknown distance unit: %s" s
    
    override _.Write(writer, value, options) =
        let str = 
            match value with
            | Miles -> "miles"
            | Kilometers -> "kilometers"
        writer.WriteStringValue(str)

// Configure in Startup.cs for server-side API
services.AddControllers()
    .AddJsonOptions(options => {
        // Only needed for event store persistence or API responses with F# types
        options.JsonSerializerOptions.Converters.Add(new DistanceUnitConverter());
    });
```

### Best Practice: Keep Blazor WASM Simple

**DON'T** reference F# domain from Blazor WASM:
```xml
❌ <ProjectReference Include="..\BikeTracking.Domain.FSharp\..." />
```

**DO** use shared C# DTOs:
```xml
✅ <ProjectReference Include="..\BikeTracking.Shared\..." />
```

---

## Conclusion

Converting the BikeTracking.Domain to F# will provide significant benefits in type safety, maintainability, and expressiveness. The current C# domain already follows functional patterns (Result type, immutable records), making it an ideal candidate for F# migration.

The discriminated union opportunities identified (9 major cases) will eliminate entire classes of runtime errors and make the domain model more explicit and self-documenting.

**Recommendation**: Proceed with migration in phases, starting with value objects and gradually moving to entities and command handlers. The investment will pay dividends in reduced bugs, easier maintenance, and more expressive domain modeling.
