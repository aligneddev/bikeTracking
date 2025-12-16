---
description: Task list for Add Ride Entry
---

# Tasks: Add Ride Entry
**Input**: Design documents from /specs/001-add-ride/
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/
**Tests**: Only include tests where requested; per constitution, propose and get approval before implementation.
**Organization**: Tasks grouped by user story for independent implementation and testing.
## Phase 1: Setup (Shared Infrastructure)
**Purpose**: Initialize solution and folders per plan.md
- [ ] T001 Create backend and frontend folders: backend/src/{domain,infrastructure,api}, frontend/src/{components,pages,services}
- [ ] T002 Initialize .NET solution with API and Blazor projects in backend/ and frontend/
- [ ] T003 [P] Configure Directory.Packages.props and pin Fluent UI Blazor v4.13.2, OpenTelemetry, EF Core
- [ ] T004 [P] Add .editorconfig and formatting rules
---
## Phase 2: Foundational (Blocking Prerequisites)
**Purpose**: Core infrastructure shared by all stories (must precede story work)
- [ ] T005 Setup EF Core DbContext and event store table (backend/src/infrastructure/ef/EventDbContext.cs)
- [ ] T006 [P] Add OpenTelemetry/Aspire wiring for API and frontend (backend/src/infrastructure/telemetry/Telemetry.cs; frontend/src/services/Telemetry.cs)
- [ ] T007 [P] Minimal API host with routing/middleware skeleton (backend/src/api/Program.cs)
- [ ] T008 Create base domain models: Ride, User (backend/src/domain/rides/Ride.cs; backend/src/domain/users/User.cs)
- [ ] T009 Configure appsettings for SQLite dev DB and placeholders for Azure SQL (backend/src/api/appsettings.Development.json)
- [ ] T010 Add DB constraints/migrations scaffold (distance/time checks, required columns) (backend/src/infrastructure/ef/Migrations/*)
**Checkpoint**: Foundation ready → user stories can start
---
## Phase 3: User Story 1 - Add a ride (Priority: P1) 🎯 MVP
**Goal**: A user can add a ride and see success + recent activity entry.
**Independent Test**: POST /rides creates event and projection; UI shows confirmation and recent ride entry.
### Implementation
- [ ] T011 [P] [US1] Blazor RideRecorder component UI with DataAnnotations (frontend/src/components/RideRecorder/RideRecorder.razor)
- [ ] T012 [US1] Client service to call POST /rides; disable submit in-flight (frontend/src/services/RideService.cs)
- [ ] T013 [US1] DTOs with DataAnnotations for command (backend/src/api/rides/CreateRideDto.cs)
- [ ] T014 [US1] Minimal API endpoint POST /rides with validation + mapping to event (backend/src/api/rides/Endpoints.cs)
- [ ] T015 [US1] Append RideRecorded to event store (backend/src/infrastructure/ef/EventStoreRepository.cs)
- [ ] T016 [US1] Build/update recent rides projection on write (backend/src/infrastructure/projections/RideProjectionUpdater.cs)
- [ ] T017 [US1] Return response with id + durationMinutes (backend/src/api/rides/Endpoints.cs)
- [ ] T018 [US1] Render recent rides list on UI (frontend/src/pages/RecentRides.razor)
### Validation & Observability
- [ ] T019 [US1] Enforce UTC timestamps and capture timezone offset (backend/src/api/rides/Endpoints.cs)
- [ ] T020 [US1] Emit OTel traces/logs/metrics for create flow (client+server)
**Checkpoint**: US1 independently functional and testable
---
## Phase 4: User Story 2 - Input validation and feedback (Priority: P2)
**Goal**: Field-specific validation messages; prevent bad data; preserve inputs.
**Independent Test**: Invalid inputs show actionable messages; corrected inputs save successfully.
### Implementation
- [ ] T021 [P] [US2] Extend Blazor validation messages per field (frontend/src/components/RideRecorder/RideRecorder.razor)
- [ ] T022 [US2] Server-side DataAnnotations + model binding errors mapped to problem details (backend/src/api/rides/Validation.cs)
- [ ] T023 [US2] Distance range and precision enforcement (backend/src/api/rides/Validation.cs)
- [ ] T024 [US2] End≥Start and no future time checks (backend/src/api/rides/Validation.cs)
- [ ] T025 [US2] Client preserves inputs on error and focuses first invalid field (frontend/src/components/RideRecorder/RideRecorder.razor)
**Checkpoint**: US2 independently functional and testable
---
## Phase 5: User Story 3 - Optional notes (Priority: P3)
**Goal**: Persist/display plain-text notes within limit; escape HTML; preserve newlines/emojis.
**Independent Test**: Notes near limit save and render correctly.
### Implementation
- [ ] T026 [P] [US3] Add notes to DTO, event, projection, and UI (backend/src/api/rides/CreateRideDto.cs; backend/src/domain/rides/RideRecorded.cs; backend/src/infrastructure/projections/RideProjection.cs; frontend/src/components/RideRecorder/RideRecorder.razor)
- [ ] T027 [US3] Escape HTML and render plain text with newlines (frontend/src/components/RideRecorder/RideRecorder.razor)
- [ ] T028 [US3] Server-side sanitization/escaping policy (backend/src/api/rides/Sanitization.cs)
**Checkpoint**: US3 independently functional and testable
---
## Phase N: Polish & Cross-Cutting
- [ ] T029 [P] Documentation updates in specs/001-add-ride/quickstart.md
- [ ] T030 Code cleanup and refactoring across backend/frontend
- [ ] T031 Performance tuning (<500ms p95; projection lag ≤5s)
- [ ] T032 [P] Add additional unit tests (approved) in backend/tests/unit and frontend/tests/e2e
- [ ] T033 Security hardening and auth stub review
- [ ] T034 [P] Validate Aspire traces/logs/metrics for create flow end-to-end
---
## Dependencies & Execution Order
- Setup → Foundational → US1 (MVP) → US2 → US3 → Polish
- After Foundational, user stories can run in parallel; recommended sequential by priority for MVP
- Within each story: Models → Services → Endpoints → UI → Observability
## Parallel Execution Examples
- [US1] Parallel: T011 (UI) + T013 (DTO) + T015 (event store) can proceed concurrently
- [US2] Parallel: T021 (UI messages) + T022 (server mapping) + T023/T024 (rules)
- [US3] Parallel: T026 (fields) + T027 (render) + T028 (server sanitization)
## Implementation Strategy
- MVP First: Complete US1, validate, demo
- Incremental: Add US2, then US3; each independently testable
## Added Tasks (Recommendations)
- [ ] T035 [US1] Prefill date and end time on form open (local current date/time).
- [ ] T036 [US1] Bind userId from session in POST /rides; add FK constraint and NOT NULL on userId.
- [ ] T037 [US1] Event store schema and versioning documentation + contract/version tests.
- [ ] T038 [US1] Observability metrics/logs/traces: latency metric, validation error structured logs, trace attributes; Application Insights exporter setup under Aspire.
- [ ] T039 [US2] DB CHECK constraints: distance range, end>=start; NOT NULL required columns.
- [ ] T040 [US3] Enforce notes maxLength (2000) client/server; verify rendering and persistence.