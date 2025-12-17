# Research Report: Bike Ride Tracking Feature

**Date**: December 15, 2025  
**Feature**: 001-ride-tracking  
**Purpose**: Resolve all NEEDS CLARIFICATION items from Technical Context

---

## 1. Weather API Selection

### Decision
**Selected: NOAA Climate Data Online (CDO) Web Service (FREE)**

### Rationale
- ✅ **FREE** - No cost for any amount of data
- ✅ **Extensive historical data** - From 1763 onwards (far exceeds 90-day requirement)
- ✅ **Government service** - Reliable, open data policy
- ✅ **REST API** - JSON/XML responses, easy .NET integration via HttpClient
- ⚠️ **Hourly data limitations** - Not all stations report hourly; data gaps will be addressed in future iterations

### Implementation Details

**Authentication**: Free API token (register at https://www.ncdc.noaa.gov/cdo-web/token)  
**Rate Limits**: 5 requests/second, 10,000 requests/day  
**Error Handling**: Graceful degradation per FR-008 (ride saves even if weather unavailable)

**C# Integration Example**:
```csharp
public class NoaaWeatherService : IWeatherService
{
    private readonly HttpClient _client;
    private readonly ILogger<NoaaWeatherService> _logger;
    private readonly string _apiToken;

    public NoaaWeatherService(HttpClient client, IConfiguration config, ILogger<NoaaWeatherService> logger)
    {
        _client = client;
        _apiToken = config["NOAA:ApiToken"];
        _logger = logger;
        _client.DefaultRequestHeaders.Add("token", _apiToken);
    }

    public async Task<WeatherData?> GetHistoricalWeatherAsync(string location, DateTime date, int hour)
    {
        try
        {
            var startDate = date.Date.AddHours(hour).ToString("yyyy-MM-ddTHH:mm:ss");
            var endDate = date.Date.AddHours(hour + 1).ToString("yyyy-MM-ddTHH:mm:ss");
            
            var response = await _client.GetFromJsonAsync<NoaaResponse>(
                $"https://www.ncdc.noaa.gov/cdo-web/api/v2/data?" +
                $"datasetid=GHCND&startdate={startDate}&enddate={endDate}&locationid={location}&units=metric");
            
            return response?.Results?.FirstOrDefault() != null 
                ? MapToWeatherData(response.Results.First()) 
                : null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Weather data unavailable for {Date} at {Hour}h", date, hour);
            return null; // Graceful degradation per FR-008
        }
    }
}
```

**Known Limitations** (deferred to future iterations):
- Hourly data not consistently available across all stations
- May require station selection logic based on user location
- Data gaps will be handled with "weather unavailable" markers

---

## 2. Database Design: Event Store Schema

### Decision
**Hybrid approach: Single event table + EF Core projections for read models**

### Event Store Schema

**Events Table** (Append-Only):
```sql
CREATE TABLE Events (
    EventId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    AggregateId UNIQUEIDENTIFIER NOT NULL,
    AggregateType NVARCHAR(100) NOT NULL,
    EventType NVARCHAR(200) NOT NULL,
    EventData NVARCHAR(MAX) NOT NULL,
    Timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    Version INT NOT NULL,
    UserId NVARCHAR(450) NOT NULL,
    CONSTRAINT CK_EventData_IsJson CHECK (ISJSON(EventData) = 1),
    INDEX IX_Events_AggregateId_Version (AggregateId, Version),
    INDEX IX_Events_Timestamp (Timestamp),
    INDEX IX_Events_UserId_Timestamp (UserId, Timestamp)
);
```

**Projection Tables** (Read Models):
```sql
CREATE TABLE RideProjections (
    RideId UNIQUEIDENTIFIER PRIMARY KEY,
    UserId NVARCHAR(450) NOT NULL,
    Date DATE NOT NULL,
    Hour INT NOT NULL CHECK (Hour BETWEEN 0 AND 23),
    Distance DECIMAL(10,2) NOT NULL,
    DistanceUnit NVARCHAR(20) NOT NULL,
    RideName NVARCHAR(200) NOT NULL,
    StartLocation NVARCHAR(200) NOT NULL,
    EndLocation NVARCHAR(200) NOT NULL,
    Notes NVARCHAR(MAX),
    WeatherData NVARCHAR(MAX),
    CreatedTimestamp DATETIME2 NOT NULL,
    ModifiedTimestamp DATETIME2,
    DeletionStatus NVARCHAR(50) NOT NULL DEFAULT 'active',
    CommunityStatus NVARCHAR(50) NOT NULL DEFAULT 'private',
    AgeInDays AS DATEDIFF(DAY, CreatedTimestamp, GETUTCDATE()) PERSISTED,
    INDEX IX_Rides_UserId_Created (UserId, CreatedTimestamp DESC),
    INDEX IX_Rides_AgeInDays (AgeInDays)
);
```

---

## 3. User Preference Persistence

### Decision
**Dedicated UserPreferences table (cross-cutting concern)**

```sql
CREATE TABLE UserPreferences (
    UserId NVARCHAR(450) PRIMARY KEY,
    DistanceUnit NVARCHAR(20) NOT NULL DEFAULT 'miles',
    CommunityOptIn BIT NOT NULL DEFAULT 0,
    CreatedTimestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ModifiedTimestamp DATETIME2,
    CONSTRAINT CK_DistanceUnit CHECK (DistanceUnit IN ('miles', 'kilometers'))
);
```

---

## 4. Community Statistics Calculation

### Decision
**Scheduled batch processing (daily) via Azure Function timer trigger**

**Implementation**: Daily at 2:00 AM UTC
**SLA**: 24-hour update window (per SC-014)
**Rationale**: Eventual consistency acceptable; reduces write latency

---

## 5. Data Deletion Request Flow

### Decision
**User-initiated self-service with 30-day approval window**

**Workflow**:
- Rides ≤ 3 months: Immediate UI deletion
- Rides > 3 months: Formal request with audit trail
- Identity verification via OAuth
- GDPR compliant with full audit logging

---

## Technology Stack Decisions Summary

| Component | Selected Technology | Rationale |
|-----------|---------------------|-----------|
| **Weather API** | NOAA CDO (FREE) | Zero cost; extensive historical data; data gaps deferred |
| **Event Store** | Azure SQL + EF Core | Append-only events; projections for reads |
| **User Preferences** | Dedicated table | Cross-cutting concern |
| **Community Stats** | Daily batch | 24h SLA acceptable |
| **Deletion Flow** | Self-service + 30-day window | GDPR compliant |

---

**Phase 0 Complete** ✅
