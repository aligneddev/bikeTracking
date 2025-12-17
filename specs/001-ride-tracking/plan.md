# Implementation Plan: Bike Ride Tracking

**Branch**: `001-ride-tracking` | **Date**: December 15, 2025 | **Spec**: [spec.md](specs/001-ride-tracking/spec.md)
**Input**: Feature specification from `/specs/001-ride-tracking/spec.md`

## Summary

Implement a comprehensive bike ride tracking system enabling users to record rides with date/hour, distance (dual-unit support), and notes. System fetches and stores weather conditions for each ride, supports ride editing, deletion (3-month window), and provides anonymized community statistics with optional future community features. GDPR-compliant with formal data deletion requests for rides older than 3 months.

## Technical Context

**Language/Version**: C# .NET 10 (latest stable)  
**Primary Dependencies**: 
- Backend: Minimal API, Entity Framework Core, Azure SQL Database
- Frontend: Blazor .NET 10, Fluent UI Blazor v4.13.2
- External: Weather API (30-90 day historical data support)

**Storage**: Azure SQL Database with Event Sourcing for audit trails  
**Testing**: xUnit, Integration tests per vertical slice, Contract tests for events  
**Target Platform**: Web (Blazor frontend + .NET Minimal API backend)  
**Project Type**: Full-stack web application (frontend + backend components)  
**Performance Goals**: API response <500ms p95, 100+ concurrent users, hourly weather data accuracy  
**Constraints**: 
- 90-day ride creation window
- 3-month individual deletion window (older rides via formal request only)
- GDPR compliance mandatory
- Hourly-only time granularity
- 95% weather data capture success rate

**Scale/Scope**: MVP with 7 user stories, 18 functional requirements, 5 primary entities, 16 success criteria

## Constitution Check

**Gate Analysis** (from project constitution):

✅ **Principle I** (Clean Architecture & DDD): Rides, Weather, User Preferences as domain aggregates; repositories abstract data access  
✅ **Principle II** (Functional Programming): Distance conversions, statistics calculations as pure functions; weather-fetch & DB as impure boundaries  
✅ **Principle III** (Event Sourcing & CQRS): All ride changes (create, edit, delete) generate immutable events; read model for leaderboards/statistics as projection  
✅ **Principle IV** (Quality-First/TDD): Full test coverage required - unit (domain logic), integration (vertical slices), contract (event schemas)  
✅ **Principle V** (UX Consistency): Fluent UI Blazor components, OAuth integration, WCAG 2.1 AA compliance, mobile-first responsive design  
✅ **Principle VI** (Performance & Observability): Structured logging to Application Insights, indexing on Ride queries (user_id, created_timestamp, age_calculation)  
✅ **Principle VII** (Data Validation): DataAnnotationsAttributes on Blazor forms + Minimal API handlers; database constraints (NOT NULL, UNIQUE, FOREIGN KEY, CHECK)  

**Approved MCP Tools**:
- `mcp_microsoft_azu2_appservice` - App Service deployment
- `mcp_microsoft_azu2_sql` - Azure SQL schema management
- `mcp_nuget_get-nuget-solver` - Package vulnerability management
- `vscode_searchExtensions_internal` - Testing framework selection

**No violations detected**. ✅ Proceed to Phase 0.

## Phase 0: Research & Clarification (BLOCKING)

All 6 clarification questions **RESOLVED** in specification session:

| Question | Answer | Impact |
|----------|--------|--------|
| Distance units? | Dual (miles/km) + user preference | User Preference entity; FR-001 updated |
| Data history window? | 90 days | 90-day constraint in FR-001, FR-007, FR-011 |
| Time granularity? | Hours only (0-23) | All weather/time logic hourly; User Story updates |
| Deletion model? | GDPR formal requests + UI deletion (3-month) | FR-012, FR-013, FR-014; New User Stories 4 & 5 |
| Social features? | Anonymous stats + optional community (future) | FR-015, FR-016, FR-017, FR-018; User Stories 6 & 7 |
| Individual deletion? | Yes, within 3 months only | FR-012 enforcement; UI dynamic delete button |

**Dependency Research** (NEEDS CLARIFICATION → must resolve):

1. **Weather API Selection**: Which historical weather API? (OpenWeatherMap, WeatherAPI, NOAA, etc.)
   - Required: 30-90 day historical hourly data, reliability 95%+, cost model
   - Impact: Affects data model (weather response structure), error handling (graceful failures)

2. **Database Design**: Event store schema for audit trail + SQL schema for normalized data?
   - Current assumption: Single event table + EF Core value converters
   - Impact: Storage footprint, query performance for leaderboards
   - Decision needed: CQRS read model caching strategy?

3. **User Preference Persistence**: User preference stored in User profile entity or Ride aggregate?
   - Current assumption: User profile (shared across features)
   - Impact: Schema design, user settings management

4. **Community Statistics Calculation**: Real-time calculation vs. scheduled batch?
   - Options: 
     - Real-time (triggers on ride changes) → immediate accuracy, higher computational cost
     - Scheduled batch (nightly) → eventual consistency, lower cost
   - Current assumption: Batch within 24 hours (SC-014)
   - Impact: Background job infrastructure, performance

5. **Data Deletion Request Flow**: Who approves? (user-initiated self-service or admin approval required?)
   - Current assumption: User-initiated with identity verification, auto-approval after 30 days
   - Impact: FR-014 implementation, compliance audit trail

---

## Phase 1: Design & Contracts

### Data Model (data-model.md)

**Entities Identified**:

| Entity | Attributes | Relationships | Constraints |
|--------|-----------|---------------|-------------|
| Ride | user_id, date, hour (0-23), distance, distance_unit, notes, weather_data, created_ts, modified_ts, deletion_status, community_status, age_in_days | User, Weather, Communities | date ≤ today (90-day window); hour 0-23 |
| Weather | temperature, conditions, wind_speed, wind_direction, humidity, pressure | Ride (1:1) | Captured hourly; null-safe when unavailable |
| UserPreference | distance_unit (miles/km), community_opt_in (bool) | User | Per-user singleton |
| DataDeletionRequest | user_id, request_ts, status (pending/approved/completed), processed_ts, audit_trail, scope | User | Identity-verified before approval |
| CommunityStatistics | total_rides, total_distance, avg_distance, ride_freq_trends, leaderboard_data | Aggregated from Rides | Computed daily; anonymized (no PII) |

**Domain Events** (Event Sourcing):

1. RideCreatedEvent(ride_id, user_id, date, hour, distance, distance_unit, notes, weather_data, timestamp)
2. RideEditedEvent(ride_id, user_id, field_changes, new_weather_data, timestamp)
3. RideDeletedEvent(ride_id, user_id, deletion_type (manual_3m or formal_request), timestamp)
4. DataDeletionRequestedEvent(user_id, scope, request_timestamp, identity_verified)
5. DataDeletionCompletedEvent(user_id, deleted_ride_ids, processed_timestamp)
6. CommunityOptInChangedEvent(user_id, opt_in_status, timestamp)
7. WeatherFetchedEvent(ride_id, weather_data, source_api, timestamp)

**Read Models** (Projections):

- `RideListProjection`: User_Id → List of Rides (for user viewing their history)
- `CommunityStatisticsProjection`: Aggregates from all rides where community_opt_in = true
- `LeaderboardProjection`: Top 100 users by distance, frequency (anonymized user IDs)
- `AuditTrailProjection`: All deletion events for compliance reporting

---

### API Contracts (contracts/)

**Minimal API Endpoints**:

```
POST   /api/rides                    → CreateRideCommand
GET    /api/rides                    → GetUserRidesQuery
GET    /api/rides/{rideId}           → GetRideDetailsQuery
PUT    /api/rides/{rideId}           → EditRideCommand
DELETE /api/rides/{rideId}           → DeleteRideCommand (only if age ≤ 3 months)
GET    /api/rides/{rideId}/weather   → GetRideWeatherQuery
POST   /api/user/preferences         → SetUserPreferencesCommand
GET    /api/user/preferences         → GetUserPreferencesQuery
POST   /api/user/data-export         → RequestDataExportCommand
GET    /api/user/data-export/{reqId} → GetExportStatusQuery
POST   /api/user/data-deletion       → RequestDataDeletionCommand
GET    /api/community/statistics     → GetCommunityStatisticsQuery
GET    /api/community/leaderboards   → GetLeaderboardsQuery
POST   /api/community/opt-in         → SetCommunityParticipationCommand
```

**DTO Examples**:

```csharp
// Request
record CreateRideRequest(
    DateOnly Date,
    int Hour,
    decimal Distance,
    string DistanceUnit, // "miles" or "kilometers"
    string? Notes
);

// Response
record RideResponse(
    Guid RideId,
    DateOnly Date,
    int Hour,
    decimal Distance,
    string DistanceUnit,
    string? Notes,
    WeatherDataResponse? Weather,
    DateTime CreatedAt,
    DateTime? ModifiedAt,
    string DeletionStatus, // "active", "marked_for_deletion"
    int AgeInDays
);

record WeatherDataResponse(
    decimal Temperature,
    string Conditions,
    decimal WindSpeed,
    string WindDirection,
    decimal Humidity,
    decimal Pressure
);

record CommunityStatisticsResponse(
    int TotalRides,
    decimal TotalDistance,
    decimal AverageDistance,
    Dictionary<string, int> RideFrequencyByMonth,
    DateTime LastUpdated
);
```

---

### Quick Start (quickstart.md)

**Local Development Setup**:

```bash
# Clone & setup
git clone <repo>
cd bikeTracking
git checkout 001-ride-tracking

# Restore packages
dotnet restore

# Run migrations (creates event store + SQL schema)
dotnet ef database update --context BikeTrackingContext

# Start Aspire orchestration (local backend + frontend)
dotnet run --project .\src\BikeTracking.AppHost

# Frontend available at https://localhost:5000
# API available at https://localhost:5001/api/...
# Swagger docs at https://localhost:5001/swagger
```

**First Test**: Create a ride and verify weather is captured:

```csharp
// Test: CreateRide_WithValidDate_StoresWeatherData
[Fact]
public async Task CreateRide_WithValidDate_StoresWeatherData()
{
    // Arrange
    var request = new CreateRideRequest(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5)), 14, 25.5m, "miles", "Great morning ride");
    
    // Act
    var response = await _client.PostAsJsonAsync("/api/rides", request);
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Created);
    var ride = await response.Content.ReadAsAsync<RideResponse>();
    ride.Weather.Should().NotBeNull();
    ride.Weather.Temperature.Should().BeGreaterThan(-50);
}
```

**Entity Framework Setup**:

```csharp
// DbContext configuration
public class BikeTrackingContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Event Store table
        builder.Entity<DomainEvent>()
            .HasKey(e => e.EventId)
            .ToTable("Events");
        
        builder.Entity<DomainEvent>()
            .Property(e => e.Data)
            .HasConversion(new JsonValueConverter()); // Store JSON

        // Ride aggregate
        builder.Entity<Ride>()
            .HasKey(r => r.RideId)
            .ToTable("Rides");
        
        builder.Entity<Ride>()
            .HasIndex(r => new { r.UserId, r.CreatedTimestamp })
            .IsUnique(false); // For fast user ride queries

        // Age calculation (database-side computed column)
        builder.Entity<Ride>()
            .Property(r => r.AgeInDays)
            .HasComputedColumnSql("DATEDIFF(day, CreatedTimestamp, GETUTCDATE())");
    }
}
```

---

## Phase 2: Implementation Roadmap

### Sprint 1: Core Ride Management (P1 User Stories 1-2)

**Deliverables**:
- ✅ Ride creation with 90-day validation + weather fetch
- ✅ Ride editing (date, hour, distance, notes)
- ✅ Ride listing (user-only view)
- ✅ Weather data storage & display
- ✅ Event sourcing for ride lifecycle

**Tests**:
- Unit: Date validation, distance unit conversion, weather response parsing (10+ tests)
- Integration: End-to-end ride CRUD (5+ tests per story)
- Contract: RideCreatedEvent schema backwards compatibility

**Estimated Effort**: 5 days

---

### Sprint 2: Deletion & Data Management (P2 User Stories 4-5)

**Deliverables**:
- ✅ Individual ride deletion (UI button for ≤3 months)
- ✅ Data export mechanism (JSON/CSV)
- ✅ Formal deletion request workflow
- ✅ Audit trail logging

**Tests**:
- Unit: Age calculation, deletion eligibility logic
- Integration: Deletion with statistics recalculation
- Contract: DataDeletionCompletedEvent integrity

**Estimated Effort**: 4 days

---

### Sprint 3: Community Features (P3 User Stories 6-7)

**Deliverables**:
- ✅ Anonymous statistics calculation (daily batch)
- ✅ Leaderboard projection (top 100 users, anonymized)
- ✅ Community opt-in/opt-out toggle
- ✅ Read model caching

**Tests**:
- Unit: Statistics aggregation algorithms (anonymization tests)
- Integration: Leaderboard accuracy across 1000+ rides
- Contract: CommunityStatisticsUpdatedEvent

**Estimated Effort**: 4 days

---

### Sprint 4: Polish, Security & Deployment

**Deliverables**:
- ✅ WCAG 2.1 AA compliance audit (Fluent UI components)
- ✅ Security testing (OAuth isolation, data access)
- ✅ Performance optimization (p95 <500ms)
- ✅ Azure deployment (SQL + Container Apps)
- ✅ Documentation & user guide

**Tests**:
- Security: Cross-user ride access prevention
- Performance: Load test 100 concurrent users
- Accessibility: Screen reader validation

**Estimated Effort**: 3 days

---

## Deliverables Summary

| Artifact | Format | Location | Status |
|----------|--------|----------|--------|
| **Specification** | Markdown | `specs/001-ride-tracking/spec.md` | ✅ Complete |
| **Implementation Plan** | Markdown | `specs/001-ride-tracking/plan.md` | ← This file |
| **Data Model** | Markdown | `specs/001-ride-tracking/data-model.md` | 📝 To generate |
| **API Contracts** | YAML/JSON | `specs/001-ride-tracking/contracts/` | 📝 To generate |
| **Quick Start** | Markdown | `specs/001-ride-tracking/quickstart.md` | 📝 To generate |
| **Task Breakdown** | Markdown | `specs/001-ride-tracking/tasks.md` | 📋 Phase 2 output |
| **Source Code** | C#/Blazor | `src/`, `tests/` | 🔨 Implementation phase |

---

## Success Metrics

| Metric | Target | Validation |
|--------|--------|-----------|
| Test Coverage | 85%+ | Code coverage report via xUnit |
| API Latency (p95) | <500ms | Load test with 100 concurrent users |
| Weather Capture Rate | 95% | Integration test over 30 rides, various dates |
| Deletion Window Enforcement | 100% | Unit test for age calculation + UI state tests |
| Community Stats Anonymity | 0% PII | Manual audit of leaderboard response |
| GDPR Audit Trail | 100% event logging | Verify all deletions in audit table |
| Mobile Responsiveness | 100% of screens | Responsive design testing (<600px, >600px) |

---

## Risk Mitigation

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|-----------|
| Weather API unreliability | Medium | High | Graceful degradation (FR-008); cache responses; fallback provider |
| Event store performance | Low | High | Indexes on (AggregateId, Version); archival strategy for 1M+ events |
| Leaderboard recalculation lag | Medium | Medium | Async batch processing; 24-hour SLA acceptable per SC-014 |
| GDPR deletion complexity | Medium | High | Formal request workflow with audit trail; legal review before launch |
| Dual-unit distance confusion | Low | Medium | User preference persists; always display unit label |

---

## Next Steps

1. **User Approval**: Review and approve implementation plan
2. **Phase 1 Execution**: Generate data-model.md, contracts/, quickstart.md
3. **Phase 2 Execution**: Create task.md with Sprint breakdown + story point estimates
4. **Development**: Parallel implementation of frontend (Blazor components) + backend (API handlers, events)

---

**Approval Checklist**:
- [ ] Technical context accurate (language, dependencies, storage)
- [ ] Constitution check passes (no unexpected violations)
- [ ] Phase 0 clarifications resolved (6/6)
- [ ] Phase 1 artifacts (data model, contracts) can be generated
- [ ] Risk mitigation acceptable
- [ ] Timeline realistic (2-3 weeks for MVP)

