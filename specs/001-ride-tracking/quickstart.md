# Quick Start Guide: Bike Ride Tracking

**Feature**: 001-ride-tracking  
**Date**: December 15, 2025

---

## Prerequisites

- **.NET 10 SDK** (latest stable)
- **Visual Studio 2025** or **VS Code** with C# extension
- **Azure SQL Database** (or SQL Server LocalDB for local development)
- **NOAA API Token** (free, register at https://www.ncdc.noaa.gov/cdo-web/token)
- **Git** for source control

---

## Local Development Setup

### 1. Clone Repository

```powershell
git clone <repository-url>
cd bikeTracking
git checkout 001-ride-tracking
```

### 2. Configure Secrets

Create ppsettings.Development.json:

```json
{
  "ConnectionStrings": {
    "BikeTrackingDb": "Server=(localdb)\\mssqllocaldb;Database=BikeTracking;Trusted_Connection=True;"
  },
  "NOAA": {
    "ApiToken": "YOUR_NOAA_API_TOKEN_HERE",
    "BaseUrl": "https://www.ncdc.noaa.gov/cdo-web/api/v2"
  },
  "Authentication": {
    "Authority": "https://login.microsoftonline.com/YOUR_TENANT_ID",
    "ClientId": "YOUR_CLIENT_ID"
  }
}
```

### 3. Restore NuGet Packages

```powershell
dotnet restore
```

### 4. Run Database Migrations

```powershell
# From project root
cd src/BikeTracking.Infrastructure
dotnet ef database update --context BikeTrackingContext
```

This creates:
- Events table (event store)
- RideProjections table (read model)
- UserPreferences table
- DataDeletionRequests table
- CommunityStatisticsProjections table

### 5. Start Aspire Orchestration

```powershell
# From project root
cd src/BikeTracking.AppHost
dotnet run
```

This launches:
- **Backend API**: https://localhost:5001
- **Blazor Frontend**: https://localhost:5000
- **Swagger UI**: https://localhost:5001/swagger

---

## First Test: Create a Ride

### Manual Test (Swagger UI)

1. Navigate to https://localhost:5001/swagger
2. Authorize with OAuth token
3. Execute POST /api/rides:

```json
{
  "date": "2025-12-10",
  "hour": 14,
  "distance": 25.5,
  "distanceUnit": "miles",
  "rideName": "Test Ride",
  "startLocation": "Home",
  "endLocation": "Office",
  "notes": "First test ride"
}
```

4. Verify response includes weather object (or null if NOAA unavailable)
5. Check database: SELECT * FROM RideProjections

### Automated Test (xUnit)

```csharp
[Fact]
public async Task CreateRide_WithValidData_StoresRideAndWeather()
{
    // Arrange
    var request = new CreateRideRequest(
        Date: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5)),
        Hour: 14,
        Distance: 25.5m,
        DistanceUnit: "miles",
        RideName: "Test Ride",
        StartLocation: "Home",
        EndLocation: "Office",
        Notes: "Integration test"
    );

    // Act
    var response = await _client.PostAsJsonAsync("/api/rides", request);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Created);
    var ride = await response.Content.ReadFromJsonAsync<RideResponse>();
    
    ride.Should().NotBeNull();
    ride.RideId.Should().NotBeEmpty();
    ride.RideName.Should().Be("Test Ride");
    ride.AgeInDays.Should().Be(5);
    
    // Weather may be null (graceful degradation)
    if (ride.Weather != null)
    {
        ride.Weather.CapturedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(5));
    }
}
```

Run tests:
```powershell
dotnet test
```

---

## Project Structure

```
bikeTracking/
├── src/
│   ├── BikeTracking.AppHost/           # Aspire orchestration
│   ├── BikeTracking.Api/               # Minimal API endpoints
│   ├── BikeTracking.Blazor/            # Frontend UI
│   ├── BikeTracking.Domain/            # Domain models, events
│   ├── BikeTracking.Infrastructure/    # EF Core, repositories
│   └── BikeTracking.Shared/            # Shared DTOs, contracts
├── tests/
│   ├── BikeTracking.UnitTests/         # Domain logic tests
│   ├── BikeTracking.IntegrationTests/  # API endpoint tests
│   └── BikeTracking.ContractTests/     # Event schema tests
└── specs/
    └── 001-ride-tracking/              # This feature documentation
        ├── spec.md
        ├── plan.md
        ├── research.md
        ├── data-model.md
        ├── quickstart.md (this file)
        └── contracts/
```

---

## Key Components

### Domain Layer

**Pure functions** (no side effects):
```csharp
// Distance conversion
public static decimal ConvertToKilometers(decimal distance, string unit)
    => unit == "miles" ? distance * 1.60934m : distance;

// 90-day validation
public static bool IsWithin90Days(DateOnly date)
    => date >= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-90))
       && date <= DateOnly.FromDateTime(DateTime.UtcNow);
```

### API Layer (Minimal API)

**Command handler** (impure boundary):
```csharp
app.MapPost("/api/rides", async (
    [FromBody] CreateRideRequest request,
    [FromServices] ICommandHandler<CreateRideCommand> handler,
    [FromServices] IWeatherService weatherService) =>
{
    // Validation
    if (!RideValidator.IsWithin90Days(request.Date))
        return Results.BadRequest("Date must be within last 90 days");

    // Fetch weather (graceful degradation)
    var weather = await weatherService.GetHistoricalWeatherAsync(
        location: "DEFAULTLOCATION", // TODO: User location preference
        date: request.Date.ToDateTime(TimeOnly.MinValue),
        hour: request.Hour
    );

    // Execute command
    var command = new CreateRideCommand(request, weather);
    var rideId = await handler.HandleAsync(command);

    return Results.Created($"/api/rides/{rideId}", rideId);
})
.RequireAuthorization();
```

### Infrastructure Layer

**EF Core DbContext**:
```csharp
public class BikeTrackingContext : DbContext
{
    public DbSet<DomainEvent> Events { get; set; }
    public DbSet<RideProjection> RideProjections { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<RideProjection>(entity =>
        {
            entity.HasKey(r => r.RideId);
            entity.HasIndex(r => new { r.UserId, r.CreatedTimestamp });
            entity.Property(r => r.AgeInDays)
                .HasComputedColumnSql("DATEDIFF(day, [CreatedTimestamp], GETUTCDATE())", stored: true);
        });
    }
}
```

---

## Testing Strategy

### Unit Tests (Pure Functions)

```csharp
[Theory]
[InlineData(10.0, "miles", 16.0934)]
[InlineData(10.0, "kilometers", 10.0)]
public void ConvertToKilometers_ReturnsCorrectValue(decimal input, string unit, decimal expected)
{
    var result = DistanceConverter.ConvertToKilometers(input, unit);
    result.Should().BeApproximately(expected, 0.001m);
}
```

### Integration Tests (Vertical Slice)

```csharp
[Fact]
public async Task CreateAndEditRide_UpdatesWeather()
{
    // Create ride
    var createRequest = new CreateRideRequest(/*...*/);
    var createResponse = await _client.PostAsJsonAsync("/api/rides", createRequest);
    var ride = await createResponse.Content.ReadFromJsonAsync<RideResponse>();

    // Edit ride (change date/hour)
    var editRequest = new EditRideRequest(Hour: 16, /*...other fields*/);
    var editResponse = await _client.PutAsJsonAsync($"/api/rides/{ride.RideId}", editRequest);
    var updatedRide = await editResponse.Content.ReadFromJsonAsync<RideResponse>();

    // Weather should be re-fetched
    updatedRide.Weather.CapturedAt.Should().BeAfter(ride.Weather.CapturedAt);
}
```

---

## Common Issues

### Weather Data Unavailable

**Symptom**: ide.Weather is null  
**Cause**: NOAA API rate limit (5 req/sec, 10K/day) or station lacks hourly data  
**Solution**: Graceful degradation per FR-008; ride still saves successfully

### Database Migration Fails

**Symptom**: dotnet ef database update error  
**Cause**: LocalDB not running or connection string incorrect  
**Solution**: Verify LocalDB instance: sqllocaldb info mssqllocaldb

### Aspire Fails to Start

**Symptom**: Orchestration error on dotnet run  
**Cause**: Port conflict (5000/5001 already in use)  
**Solution**: Check ports: 
etstat -ano | findstr :5000

---

## Deployment (Azure)

### Prerequisites

- Azure subscription
- Azure CLI installed
- Azure Developer CLI (azd) installed

### Deploy to Azure Container Apps

```powershell
# Login
az login
azd auth login

# Initialize
azd init

# Provision + Deploy
azd up
```

This provisions:
- Azure SQL Database (serverless)
- Azure Container Apps (API + Blazor)
- Application Insights
- Azure Key Vault (secrets)

---

## Next Steps

1. ✅ Local setup complete
2. Run integration tests: dotnet test
3. Review [data-model.md](data-model.md) for entity schemas
4. Review [contracts/README.md](contracts/README.md) for API endpoints
5. Implement Phase 2: User Stories 4-5 (deletion features)

---

**Phase 1: Quick Start Complete** ✅
