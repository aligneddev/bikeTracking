# Tasks: Bike Ride Tracking

**Feature**: 001-ride-tracking  
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/, quickstart.md  
**Tests**: NOT requested in spec - test tasks EXCLUDED per constitution TDD workflow

**Organization**: Tasks grouped by user story for independent implementation and testing

---

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: User story label (US1, US2, US3, etc.)
- All file paths use .NET 10 web app structure: `src/BikeTracking.*`, `tests/BikeTracking.*`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization

- [x] T001 Create .NET 10 solution structure with Aspire AppHost at src/BikeTracking.AppHost/Program.cs
- [x] T002 [P] Initialize Blazor frontend project at src/BikeTracking.Blazor/ with Fluent UI v4.13.2
- [x] T003 [P] Initialize Minimal API project at src/BikeTracking.Api/Program.cs
- [x] T004 [P] Initialize Domain project at src/BikeTracking.Domain/ (class library)
- [x] T005 [P] Initialize Infrastructure project at src/BikeTracking.Infrastructure/ (EF Core)
- [x] T006 [P] Initialize Shared project at src/BikeTracking.Shared/ (DTOs, contracts)
- [x] T007 [P] Add NuGet packages: EF Core, Fluent UI Blazor, Azure.Identity, Application Insights SDK
- [x] T008 Configure linting (EditorConfig) and code analysis rules at root .editorconfig

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure MUST be complete before ANY user story

**WARNING: No user story work can begin until this phase is complete**

- [x] T009 Setup Azure SQL database schema migrations in src/BikeTracking.Infrastructure/Migrations/
- [x] T010 [P] Configure OAuth authentication with Microsoft.AspNetCore.Authentication.OpenIdConnect in src/BikeTracking.Api/Program.cs
- [x] T011 [P] Create DbContext at src/BikeTracking.Infrastructure/BikeTrackingContext.cs with Events, RideProjections, UserPreferences, DataDeletionRequests, CommunityStatisticsProjections tables
- [x] T012 [P] Implement event store repository at src/BikeTracking.Infrastructure/Repositories/EventStoreRepository.cs
- [x] T013 [P] Configure Application Insights logging middleware in src/BikeTracking.Api/Program.cs
- [x] T014 [P] Create base domain event types at src/BikeTracking.Domain/Events/DomainEvent.cs
- [x] T015 [P] Implement NOAA weather service interface IWeatherService at src/BikeTracking.Domain/Services/IWeatherService.cs
- [x] T016 Implement NOAA weather service at src/BikeTracking.Infrastructure/Services/NoaaWeatherService.cs with HttpClient + graceful degradation (FR-008)
- [x] T017 [P] Create Fluent UI DesignTheme component at src/BikeTracking.Blazor/Shared/DesignTheme.razor with brand palette (FFCDA4, FFB170, FF7400, D96200, A74C00)
- [x] T018 [P] Setup appsettings.Development.json with NOAA API token, OAuth config, connection strings
- [x] T019 Configure Aspire orchestration at src/BikeTracking.AppHost/Program.cs with API + Blazor + SQL dependencies
- [x] T020 Run initial EF Core migration: dotnet ef database update to create Events, RideProjections, UserPreferences tables

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Add a New Bike Ride (Priority: P1) MVP

**Goal**: Users can record rides with date, hour, distance, ride name, start/end locations, notes. Weather data fetched and stored automatically.

**Independent Test**: Add a ride with all details verify saved with weather data attached

### Implementation for User Story 1

- [x] T021 [P] [US1] Create Ride entity at src/BikeTracking.Domain/Entities/Ride.cs with validation rules (date today, today-90, hour 0-23, required fields)
- [x] T022 [P] [US1] Create Weather value object at src/BikeTracking.Domain/ValueObjects/Weather.cs (nullable fields for graceful degradation)
- [x] T023 [P] [US1] Create RideCreated domain event at src/BikeTracking.Domain/Events/RideCreated.cs
- [x] T024 [P] [US1] Create WeatherFetched domain event at src/BikeTracking.Domain/Events/WeatherFetched.cs
- [x] T025 [P] [US1] Create WeatherFetchFailed domain event at src/BikeTracking.Domain/Events/WeatherFetchFailed.cs
- [x] T026 [P] [US1] Create CreateRideRequest DTO at src/BikeTracking.Shared/DTOs/CreateRideRequest.cs with DataAnnotations validation
- [x] T027 [P] [US1] Create RideResponse DTO at src/BikeTracking.Shared/DTOs/RideResponse.cs
- [x] T028 [US1] Implement CreateRideCommand handler at src/BikeTracking.Domain/Commands/CreateRideCommandHandler.cs (pure function: request events)
- [x] T029 [US1] Implement RideProjection repository at src/BikeTracking.Infrastructure/Repositories/RideProjectionRepository.cs (saves to RideProjections table)
- [x] T030 [US1] Implement POST /api/rides endpoint at src/BikeTracking.Api/Endpoints/RidesEndpoints.cs with OAuth requirement, weather fetch, event append
- [x] T031 [US1] Add computed column AgeInDays to RideProjections via EF Core migration at src/BikeTracking.Infrastructure/Migrations/AddAgeInDaysColumn.cs
- [x] T032 [US1] Create RideRecorder.razor Blazor component at src/BikeTracking.Blazor/Pages/RideRecorder.razor with Fluent UI form, DataAnnotations validation
- [x] T033 [US1] Add date picker (90-day constraint client-side) in RideRecorder.razor
- [x] T034 [US1] Add hour dropdown (0-23) in RideRecorder.razor
- [x] T035 [US1] Add distance input with unit toggle (miles/km) in RideRecorder.razor
- [x] T036 [US1] Add ride name, start location, end location text inputs (200 char limit) in RideRecorder.razor
- [x] T037 [US1] Add notes textarea in RideRecorder.razor
- [x] T038 [US1] Implement weather display in RideRecorder.razor (shows fetched weather preview or "unavailable" message)
- [x] T039 [US1] Wire up form submit to POST /api/rides with error handling in RideRecorder.razor

**Checkpoint**: Users can successfully add rides with all required fields and weather data

---

## Phase 4: User Story 2 - Edit an Existing Ride (Priority: P1) MVP

**Goal**: Users can update any ride detail (date, hour, distance, name, locations, notes). Weather re-fetched if date/hour changed.

**Independent Test**: Edit a saved ride verify changes persisted + weather updated if date/hour changed

### Implementation for User Story 2

- [x] T040 [P] [US2] Create RideEdited domain event at src/BikeTracking.Domain/Events/RideEdited.cs (captures changed fields JSON)
- [x] T041 [P] [US2] Create EditRideRequest DTO at src/BikeTracking.Shared/DTOs/EditRideRequest.cs with DataAnnotations validation
- [x] T042 [P] [US2] Create GetUserRidesQuery DTO at src/BikeTracking.Shared/DTOs/GetUserRidesQuery.cs (pagination support)
- [x] T043 [P] [US2] Create RideListItemResponse DTO at src/BikeTracking.Shared/DTOs/RideListItemResponse.cs (summary view: name, locations, distance, age)
- [x] T044 [US2] Implement EditRideCommand handler at src/BikeTracking.Domain/Commands/EditRideCommandHandler.cs (detects changed fields, optionally fetches weather)
- [x] T045 [US2] Implement GET /api/rides endpoint at src/BikeTracking.Api/Endpoints/RidesEndpoints.cs (returns user's rides with pagination)
- [x] T046 [US2] Implement GET /api/rides/{rideId} endpoint at src/BikeTracking.Api/Endpoints/RidesEndpoints.cs (returns full ride details)
- [x] T047 [US2] Implement PUT /api/rides/{rideId} endpoint at src/BikeTracking.Api/Endpoints/RidesEndpoints.cs (validates 90-day constraint, re-fetches weather if date/hour changed)
- [x] T048 [US2] Create RideList.razor Blazor component at src/BikeTracking.Blazor/Pages/RideList.razor (displays rides with name, start, end, distance, age)
- [x] T049 [US2] Add pagination controls (prev/next, page size selector) to RideList.razor
- [x] T050 [US2] Add "Edit" button per ride in RideList.razor navigating to RideEditor.razor
- [x] T051 [US2] Create RideEditor.razor Blazor component at src/BikeTracking.Blazor/Pages/RideEditor.razor (loads existing ride, pre-fills form)
- [x] T052 [US2] Implement cancel button in RideEditor.razor (navigates back without saving)
- [x] T053 [US2] Implement save button in RideEditor.razor calling PUT /api/rides/{rideId} with change detection
- [x] T054 [US2] Add weather re-fetch indicator in RideEditor.razor when date/hour changed

**Checkpoint**: Users can view ride list and edit any ride with all changes persisted

---

## Phase 5: User Story 3 - View Weather Data for a Ride (Priority: P2)

**Goal**: Users viewing ride details see weather conditions (temp, conditions, wind, humidity, pressure) from when ride was recorded.

**Independent Test**: View a saved ride confirm weather information displayed correctly

### Implementation for User Story 3

- [x] T055 [P] [US3] Create WeatherDisplay.razor Blazor component at src/BikeTracking.Blazor/Shared/WeatherDisplay.razor (formats temperature, conditions, wind, humidity, pressure)
- [x] T056 [US3] Create RideDetails.razor Blazor page at src/BikeTracking.Blazor/Pages/RideDetails.razor (calls GET /api/rides/{rideId})
- [x] T057 [US3] Add WeatherDisplay component to RideDetails.razor (shows weather or "unavailable" message if null)
- [x] T058 [US3] Add navigation from RideList.razor to RideDetails.razor (clickable ride name)
- [x] T059 [US3] Add back button in RideDetails.razor to return to RideList

**Checkpoint**: Users can view full ride details including weather context

---


---

## 🔄 DEFERRED TO FUTURE SPEC

**Phases 6-9 have been deferred to a separate specification** to maintain focus on the core ride tracking MVP. The following user stories will be implemented in spec `002-data-management-and-community`:

- **Phase 6**: User Story 4 - Delete Individual Rides (8 tasks)
- **Phase 7**: User Story 5 - Request Personal Data Deletion (17 tasks)
- **Phase 8**: User Story 6 - View Anonymous Statistics & Leaderboards (18 tasks)
- **Phase 9**: User Story 7 - Optional Community Features (4 tasks)

**Total Deferred: 47 tasks**

---
## Phase 6: User Story 4 - Delete Individual Rides (Priority: P2)

**Goal**: Users can delete rides 90 days old via UI. Rides > 90 days show disabled delete button with explanation.

**Independent Test**: Delete a recent ride verify removed from list + deletion event logged. Try deleting old ride verify button disabled.

### Implementation for User Story 4

- [ ] T060 [P] [US4] Create RideDeleted domain event at src/BikeTracking.Domain/Events/RideDeleted.cs (includes deletion_type: manual_3m or formal_request)
- [ ] T061 [US4] Implement DeleteRideCommand handler at src/BikeTracking.Domain/Commands/DeleteRideCommandHandler.cs (soft delete: marks DeletionStatus = deleted)
- [ ] T062 [US4] Implement DELETE /api/rides/{rideId} endpoint at src/BikeTracking.Api/Endpoints/RidesEndpoints.cs (validates AgeInDays 90, returns 403 Forbidden if older)
- [ ] T063 [US4] Add delete button to RideList.razor (enabled/disabled based on AgeInDays 90)
- [ ] T064 [US4] Add delete confirmation dialog in RideList.razor (are you sure?)
- [ ] T065 [US4] Add tooltip to disabled delete button explaining 90-day constraint
- [ ] T066 [US4] Filter deleted rides from GET /api/rides query (WHERE DeletionStatus = 'active')
- [ ] T067 [US4] Update community statistics when ride deleted (trigger Azure Function or mark stats for recalculation)

**Checkpoint**: Users can delete recent rides; older rides protected with clear feedback

---

## Phase 7: User Story 5 - Request Personal Data Deletion (Priority: P2)

**Goal**: Users can export their data (JSON/CSV) and submit deletion requests for rides > 90 days. GDPR compliant with audit trail.

**Independent Test**: Request export receive file. Submit deletion request verify rides > 90 days marked for deletion after 30-day window.

### Implementation for User Story 5

- [ ] T068 [P] [US5] Create DataDeletionRequest entity at src/BikeTracking.Domain/Entities/DataDeletionRequest.cs
- [ ] T069 [P] [US5] Create DataDeletionRequested event at src/BikeTracking.Domain/Events/DataDeletionRequested.cs
- [ ] T070 [P] [US5] Create DataDeletionCompleted event at src/BikeTracking.Domain/Events/DataDeletionCompleted.cs
- [ ] T071 [P] [US5] Create DataExportRequested event at src/BikeTracking.Domain/Events/DataExportRequested.cs
- [ ] T072 [P] [US5] Create DataExportCompleted event at src/BikeTracking.Domain/Events/DataExportCompleted.cs
- [ ] T073 [P] [US5] Create RequestDataExportRequest DTO at src/BikeTracking.Shared/DTOs/RequestDataExportRequest.cs
- [ ] T074 [P] [US5] Create RequestDataDeletionRequest DTO at src/BikeTracking.Shared/DTOs/RequestDataDeletionRequest.cs (scope: older_than_3_months | full_account)
- [ ] T075 [US5] Implement POST /api/user/data-export endpoint at src/BikeTracking.Api/Endpoints/UserDataEndpoints.cs (creates export request, returns 202 Accepted)
- [ ] T076 [US5] Implement GET /api/user/data-export/{requestId} endpoint at src/BikeTracking.Api/Endpoints/UserDataEndpoints.cs (checks status, returns download URL when complete)
- [ ] T077 [US5] Implement POST /api/user/data-deletion endpoint at src/BikeTracking.Api/Endpoints/UserDataEndpoints.cs (validates identity, logs deletion request event)
- [ ] T078 [US5] Create Azure Function at src/BikeTracking.Functions/ProcessDataExportFunction.cs (timer trigger: nightly, exports user data to JSON/CSV)
- [ ] T079 [US5] Create Azure Function at src/BikeTracking.Functions/ProcessDataDeletionFunction.cs (timer trigger: daily, processes requests > 30 days old)
- [ ] T080 [US5] Implement data export logic in ProcessDataExportFunction.cs (queries all user rides, preferences, formats as JSON, uploads to Azure Blob Storage)
- [ ] T081 [US5] Implement data deletion logic in ProcessDataDeletionFunction.cs (soft deletes rides > 90 days, logs DataDeletionCompleted event)
- [ ] T082 [US5] Create AccountSettings.razor page at src/BikeTracking.Blazor/Pages/AccountSettings.razor with "Export Data" and "Delete Data" buttons
- [ ] T083 [US5] Add export status polling in AccountSettings.razor (checks GET /api/user/data-export/{requestId} every 5 seconds)
- [ ] T084 [US5] Add deletion request confirmation dialog in AccountSettings.razor (explains 30-day processing window)

**Checkpoint**: Users can export data and submit GDPR-compliant deletion requests

---

## Phase 8: User Story 6 - View Anonymous Statistics & Leaderboards (Priority: P3)

**Goal**: Users see anonymized community stats (total rides, avg distance, trends) and leaderboards (top 100 by distance/frequency). 0% PII exposed.

**Independent Test**: View community stats verify no PII. Opt out verify user removed from stats within 1 hour.

### Implementation for User Story 6

- [ ] T085 [P] [US6] Create UserPreference entity at src/BikeTracking.Domain/Entities/UserPreference.cs (DistanceUnit, CommunityOptIn)
- [ ] T086 [P] [US6] Create CommunityStatistics entity at src/BikeTracking.Domain/Entities/CommunityStatistics.cs
- [ ] T087 [P] [US6] Create CommunityOptInChanged event at src/BikeTracking.Domain/Events/CommunityOptInChanged.cs
- [ ] T088 [P] [US6] Create CommunityStatisticsUpdated event at src/BikeTracking.Domain/Events/CommunityStatisticsUpdated.cs
- [ ] T089 [P] [US6] Create GetUserPreferencesQuery DTO at src/BikeTracking.Shared/DTOs/GetUserPreferencesQuery.cs
- [ ] T090 [P] [US6] Create SetUserPreferencesRequest DTO at src/BikeTracking.Shared/DTOs/SetUserPreferencesRequest.cs
- [ ] T091 [P] [US6] Create CommunityStatisticsResponse DTO at src/BikeTracking.Shared/DTOs/CommunityStatisticsResponse.cs
- [ ] T092 [P] [US6] Create LeaderboardResponse DTO at src/BikeTracking.Shared/DTOs/LeaderboardResponse.cs (anonymized user IDs)
- [ ] T093 [US6] Implement GET /api/user/preferences endpoint at src/BikeTracking.Api/Endpoints/UserPreferencesEndpoints.cs
- [ ] T094 [US6] Implement POST /api/user/preferences endpoint at src/BikeTracking.Api/Endpoints/UserPreferencesEndpoints.cs (updates DistanceUnit, CommunityOptIn)
- [ ] T095 [US6] Implement GET /api/community/statistics endpoint at src/BikeTracking.Api/Endpoints/CommunityEndpoints.cs (returns aggregate totals, averages, trends)
- [ ] T096 [US6] Implement GET /api/community/leaderboards endpoint at src/BikeTracking.Api/Endpoints/CommunityEndpoints.cs (top 100 by metric: distance | frequency)
- [ ] T097 [US6] Create Azure Function at src/BikeTracking.Functions/UpdateCommunityStatisticsFunction.cs (timer trigger: daily 2 AM UTC)
- [ ] T098 [US6] Implement statistics calculation logic in UpdateCommunityStatisticsFunction.cs (pure function: aggregates opted-in rides, anonymizes user IDs via hash)
- [ ] T099 [US6] Create CommunityStats.razor page at src/BikeTracking.Blazor/Pages/CommunityStats.razor displaying totals, averages, trends chart
- [ ] T100 [US6] Create Leaderboard.razor component at src/BikeTracking.Blazor/Shared/Leaderboard.razor (top 100 table with anonymized IDs)
- [ ] T101 [US6] Add opt-in/opt-out toggle to AccountSettings.razor calling POST /api/user/preferences
- [ ] T102 [US6] Add real-time opt-out handling: Azure Function triggers on CommunityOptInChanged event, recalculates stats within 1 hour

**Checkpoint**: Community features functional with 0% PII leakage, opt-in/opt-out enforced

---

## Phase 9: User Story 7 - Optional Community Features (Priority: P3 - Future)

**Goal**: Infrastructure for future community features (shareable rides, challenges). NOT implemented in MVP, but opt-in flags and visibility markers present.

**Independent Test**: Verify CommunityStatus field exists on rides. Verify opt-in mechanism functional even if sharing not yet available.

### Implementation for User Story 7

- [ ] T103 [P] [US7] Add CommunityStatus column to RideProjections table via EF Core migration at src/BikeTracking.Infrastructure/Migrations/AddCommunityStatusColumn.cs (values: private | shareable | public)
- [ ] T104 [P] [US7] Update Ride entity to include CommunityStatus property at src/BikeTracking.Domain/Entities/Ride.cs (defaults to 'private')
- [ ] T105 [US7] Add "Make Shareable" button to RideDetails.razor (disabled with tooltip: "Coming soon")
- [ ] T106 [US7] Document future community features architecture in specs/001-ride-tracking/future-community-features.md

**Checkpoint**: Infrastructure ready for future community features; MVP complete without implementation

---

## Phase 10: Polish & Cross-Cutting Concerns

**Purpose**: Final improvements affecting multiple user stories

- [ ] T107 [P] Add structured logging for all ride mutations (create, edit, delete) via Application Insights in src/BikeTracking.Api/Middleware/LoggingMiddleware.cs
- [ ] T108 [P] Implement audit event log queries GET /api/audit/events endpoint at src/BikeTracking.Api/Endpoints/AuditEndpoints.cs (filters: date range, event type, user ID)
- [ ] T109 [P] Add WCAG 2.1 AA compliance audit: keyboard navigation, color contrast, screen reader support in all Blazor components
- [ ] T110 [P] Add mobile responsive design testing (breakpoints: 600px mobile, 601-1024px tablet, >1024px desktop)
- [ ] T111 [P] Implement caching for GET /api/community/statistics (24-hour cache per SC-014)
- [ ] T112 [P] Add rate limiting middleware for NOAA API calls (5 req/sec max) in NoaaWeatherService.cs
- [ ] T113 [P] Add performance monitoring: API response time <500ms p95 via Application Insights alerts
- [ ] T114 [P] Add error boundary components in Blazor app at src/BikeTracking.Blazor/Shared/ErrorBoundary.razor
- [ ] T115 Code cleanup: Remove unused using statements, apply consistent naming conventions
- [x] T116 Run quickstart.md validation: Follow setup steps, verify first test passes
- [ ] T117 Generate API documentation: OpenAPI/Swagger JSON export to specs/001-ride-tracking/contracts/openapi.json
- [x] T118 [P] Add README.md to repository root with setup instructions, architecture diagram, contribution guidelines

**Checkpoint**: All polish tasks complete, MVP ready for deployment

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - start immediately
- **Foundational (Phase 2)**: Depends on Setup - BLOCKS all user stories
- **User Stories (Phases 3-9)**: All depend on Foundational completion
  - US1, US2 (P1) Core MVP, implement first
  - US3, US4, US5 (P2) Secondary features, can implement after US1-2 or in parallel if staffed
  - US6, US7 (P3) Future enhancements, defer after MVP
- **Polish (Phase 10)**: Depends on all desired user stories

### User Story Dependencies

- **US1 (Add Ride)**: No dependencies on other stories - implement first
- **US2 (Edit Ride)**: Minimal dependency on US1 (reuses Ride entity, endpoints) but independently testable
- **US3 (View Weather)**: Depends on US1 (needs rides with weather data)
- **US4 (Delete Rides)**: Depends on US1 (needs rides to delete), US2 (reuses ride list)
- **US5 (Data Deletion)**: Depends on US1 (needs rides to export/delete)
- **US6 (Community Stats)**: Depends on US1 (aggregates rides), US4 (handles deleted rides)
- **US7 (Future Community)**: No implementation dependencies (infrastructure only)

### Parallel Opportunities

**Within Setup (Phase 1)**: T002-T007 can run in parallel (different projects)

**Within Foundational (Phase 2)**: T010-T018 can run in parallel (independent concerns)

**Across User Stories** (after Foundational complete):
- US1 + US6 can proceed in parallel (different domains)
- US2 + US3 + US4 can proceed in parallel (different features)
- US5 (Data Deletion) should wait for US1-4 to validate export/deletion logic

**Within Each User Story**:
- Entity/DTO/Event creation tasks marked [P] run in parallel
- API endpoints can be built in parallel with Blazor pages (contract-driven)

---

## Suggested MVP Scope

**Minimal Viable Product (deliver first)**:
- Phase 1: Setup
- Phase 2: Foundational
- Phase 3: User Story 1 (Add Ride)
- Phase 4: User Story 2 (Edit Ride)
- Phase 10: Polish (T116 quickstart validation only)

**Total MVP Tasks**: T001-T054 + T116 = **55 tasks**

**Post-MVP Enhancements** (deliver incrementally):
- Phase 5: User Story 3 (View Weather) - 5 tasks
- Phase 6: User Story 4 (Delete Rides) - 8 tasks
- Phase 7: User Story 5 (Data Deletion) - 17 tasks
- Phase 8: User Story 6 (Community Stats) - 18 tasks
- Phase 9: User Story 7 (Future Community) - 4 tasks
- Phase 10: Polish (remaining) - 11 tasks

**Total Feature Tasks**: **118 tasks**

---

## Task Count Summary

- **Phase 1 (Setup)**: 8 tasks
- **Phase 2 (Foundational)**: 12 tasks (BLOCKING)
- **Phase 3 (US1 - Add Ride)**: 19 tasks MVP
- **Phase 4 (US2 - Edit Ride)**: 15 tasks MVP
- **Phase 5 (US3 - View Weather)**: 5 tasks
- **Phase 6 (US4 - Delete Rides)**: 8 tasks
- **Phase 7 (US5 - Data Deletion)**: 17 tasks
- **Phase 8 (US6 - Community Stats)**: 18 tasks
- **Phase 9 (US7 - Future Community)**: 4 tasks
- **Phase 10 (Polish)**: 12 tasks

**Total**: **118 tasks** organized across 10 phases

---

**Format Validation**: All tasks follow checklist format with [ID], [P] (if parallel), [Story] (if user story), and file paths

**Independent Testing**: Each user story can be tested independently per acceptance criteria in spec.md

**Implementation Strategy**: MVP-first (US1-US2), incremental delivery (US3-US7), parallelizable within constraints






