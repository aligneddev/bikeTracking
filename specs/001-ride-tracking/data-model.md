# Data Model: Bike Ride Tracking

**Feature**: 001-ride-tracking  
**Date**: December 15, 2025

---

## Domain Entities

### 1. Ride (Aggregate Root)

**Purpose**: Represents a single bike ride with weather conditions

**Attributes**:
- RideId (Guid, PK) - Unique identifier
- UserId (string, FK) - Owner of the ride (OAuth identity)
- Date (DateOnly) - Ride date (within last 90 days)
- Hour (int) - Hour of ride (0-23, hourly granularity)
- Distance (decimal) - Distance traveled
- DistanceUnit (string) - 'miles' or 'kilometers'
- RideName (string) - User-defined ride name (max 200 chars)
- StartLocation (string) - Start location name (max 200 chars)
- EndLocation (string) - End location name (max 200 chars)
- Notes (string, nullable) - User notes
- WeatherData (Weather, value object) - Captured weather conditions
- CreatedTimestamp (DateTime, UTC) - Creation time
- ModifiedTimestamp (DateTime, UTC, nullable) - Last edit time
- DeletionStatus (enum) - active | marked_for_deletion
- CommunityStatus (enum) - private | shareable | public (future)
- AgeInDays (computed) - Days since creation

**Validation Rules**:
- Date must be ≤ today and ≥ today - 90 days (FR-007, FR-011)
- Hour must be 0-23 (FR-007)
- Distance > 0
- DistanceUnit must be 'miles' or 'kilometers' (FR-001)
- RideName, StartLocation, EndLocation required (FR-001)
- Character limits: RideName, StartLocation, EndLocation ≤ 200 chars

**Invariants**:
- A ride can only be deleted via UI if AgeInDays ≤ 90 (FR-012)
- Rides > 90 days require formal deletion request (FR-014)

---

### 2. Weather (Value Object)

**Purpose**: Immutable weather conditions at time of ride

**Attributes**:
- Temperature (decimal, nullable) - Degrees (unit based on API)
- Conditions (string, nullable) - Sunny, cloudy, rainy, etc.
- WindSpeed (decimal, nullable) - Wind speed
- WindDirection (string, nullable) - N, NE, E, SE, S, SW, W, NW
- Humidity (decimal, nullable) - Humidity percentage
- Pressure (decimal, nullable) - Atmospheric pressure
- CapturedAt (DateTime, UTC) - When weather was fetched

**Validation Rules**:
- All fields nullable (graceful degradation per FR-008)
- If CapturedAt is null, weather fetch failed

---

### 3. UserPreference (Aggregate Root)

**Purpose**: User-level settings (distance unit, community opt-in)

**Attributes**:
- UserId (string, PK) - OAuth identity
- DistanceUnit (string) - 'miles' or 'kilometers'
- CommunityOptIn (bool) - Consent for community features
- CreatedTimestamp (DateTime, UTC)
- ModifiedTimestamp (DateTime, UTC, nullable)

**Validation Rules**:
- DistanceUnit must be 'miles' or 'kilometers'
- Singleton per user

---

### 4. DataDeletionRequest (Aggregate Root)

**Purpose**: GDPR data deletion request tracking

**Attributes**:
- RequestId (Guid, PK)
- UserId (string, FK)
- RequestTimestamp (DateTime, UTC)
- Status (enum) - pending | approved | completed
- Scope (enum) - older_than_3_months | full_account
- ProcessedTimestamp (DateTime, UTC, nullable)
- AuditTrail (JSON) - Verification steps, actions taken

**Validation Rules**:
- Status must be valid enum value
- Scope must be valid enum value
- ProcessedTimestamp required when Status = completed

---

### 5. CommunityStatistics (Aggregate Root)

**Purpose**: Pre-computed anonymous aggregate metrics

**Attributes**:
- StatisticId (Guid, PK)
- TotalRides (int) - Count of all opted-in rides
- TotalDistance (decimal) - Sum distance (normalized to km)
- AverageDistance (decimal) - Mean distance per ride
- RideFrequencyTrends (JSON) - {month: count} dictionary
- LeaderboardData (JSON) - Top 100 anonymized users
- LastUpdated (DateTime, UTC) - Last computation time

**Validation Rules**:
- 0% PII in LeaderboardData (SC-013)
- User identifiers hashed/pseudonymized
- Updated daily via batch process (SC-014)

---

## Domain Events

### Event Store Structure

All events stored in Events table with:
- EventId (Guid, PK)
- AggregateId (Guid) - Which entity changed
- AggregateType (string) - 'Ride', 'UserPreference', etc.
- EventType (string) - Event name
- EventData (JSON) - Serialized payload
- Timestamp (DateTime, UTC)
- Version (int) - Optimistic concurrency
- UserId (string) - Actor

### Event Catalog

**Ride Lifecycle**:
1. RideCreated - New ride added
2. RideEdited - Ride updated (includes changed fields)
3. RideDeleted - Ride removed (manual or formal request)

**Weather**:
4. WeatherFetched - Weather data retrieved
5. WeatherFetchFailed - Weather unavailable (graceful degradation)

**User Preferences**:
6. UserPreferenceChanged - Distance unit or opt-in toggled

**Community**:
7. CommunityOptInChanged - User opted in/out
8. CommunityStatisticsUpdated - Daily batch complete

**Data Management**:
9. DataDeletionRequested - User submitted deletion request
10. DataDeletionCompleted - Deletion processed

**Audit**:
11. DataExportRequested - User requested data export
12. DataExportCompleted - Export delivered

---

## Entity Relationships

```
User (OAuth)
 ├─ 1:N → Ride
 ├─ 1:1 → UserPreference
 └─ 1:N → DataDeletionRequest

Ride
 ├─ 1:1 → Weather (value object)
 └─ N:1 → User

CommunityStatistics
 └─ (computed from Rides where User.CommunityOptIn = true)
```

---

## Database Schema

### Tables

1. **Events** - Event sourcing store (append-only)
2. **RideProjections** - Materialized view for queries
3. **UserPreferences** - User settings
4. **DataDeletionRequests** - GDPR tracking
5. **CommunityStatisticsProjections** - Pre-computed aggregates

### Indexes

**RideProjections**:
- IX_Rides_UserId_Created (UserId, CreatedTimestamp DESC) - User ride list
- IX_Rides_AgeInDays (AgeInDays) - Deletion eligibility queries

**Events**:
- IX_Events_AggregateId_Version - Event replay
- IX_Events_Timestamp - Audit queries
- IX_Events_UserId_Timestamp - User activity audit

---

## Validation Strategy (Per Constitution Principle VII)

### Client-Side (Blazor)
- DataAnnotations on form models
- Real-time feedback to user

### Server-Side (Minimal API)
- DataAnnotations on DTOs
- Command handler validation

### Database-Side
- CHECK constraints (Hour 0-23, DistanceUnit enum)
- NOT NULL constraints (required fields)
- UNIQUE constraints where applicable
- FOREIGN KEY constraints (referential integrity)

---

## State Transitions

### Ride Lifecycle

```
[Created] → (User edits) → [Modified]
          → (Age ≤ 90 days + user deletes) → [Deleted]
          → (Age > 90 days + formal request) → [Marked for Deletion] → [Deleted]
```

### Deletion Request Lifecycle

```
[Pending] → (30 days + identity verified) → [Approved] → [Completed]
```

---

**Phase 1: Data Model Complete** ✅
