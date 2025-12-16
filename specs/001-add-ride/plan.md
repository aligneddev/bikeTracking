# Implementation Plan: Add Ride Entry

**Branch**: 001-add-ride | **Date**: 2025-12-16 | **Spec**: specs/001-add-ride/spec.md
**Input**: Feature specification from /specs/001-add-ride/spec.md

## Summary
Enable authenticated users to add a ride with date, start/end time, distance (miles), and optional notes. Blazor client with DataAnnotations; Minimal API on .NET 10; event-sourced RideRecorded and read projection. UTC timestamps with timezone offset; OpenTelemetry via Aspire.

## Technical Context
Language/Version: C# (.NET 10)
Primary Dependencies: Blazor, Minimal API, EF Core, Fluent UI Blazor v4.13.2, Application Insights, Aspire, OpenTelemetry
Storage: Azure SQL (event store + read projections); local dev DB NEEDS CLARIFICATION
Testing: xUnit (unit/integration), Playwright MCP (E2E), event contract tests
Target Platform: Web (Blazor + API), Azure Container Apps
Project Type: Web application (frontend + backend + projection)
Performance Goals: API p95 <500ms; projection lag <5s
Constraints: WCAG 2.1 AA; validation client/server/DB; observability per FR-014
Scale/Scope: Single-user MVP; multi-user later

## Constitution Check
Clean Architecture: PASS
Functional Core/Impure Shell: PASS
Event Sourcing & CQRS: PASS
Quality-First Development: PASS (tests to be proposed in Phase 2)
UX & Accessibility: PASS
Performance & Observability: PASS
Data Validation & Integrity: PASS

## Project Structure
Documentation (this feature): specs/001-add-ride/{plan.md,research.md,data-model.md,quickstart.md,contracts/}
Source Code: backend/src/{domain,infra,api}/; frontend/src/{components,pages,services}/; tests for unit/integration/contract/e2e
Structure Decision: Web app with separate frontend and backend plus projection component.

## Complexity Tracking
Violation: Event Sourcing & CQRS | Why Needed: Auditability | Alternative Rejected: Simple CRUD lacks history and scalability\n\n## Phase 2: Proposed Tests (for approval)\n\n### Unit\n- RideValidation: required fields; distance range (0.1–1000.0); end ≥ start; reject future times.\n- DurationCalc: correct minutes across midnight/DST; start=end edge.\n- NotesSanitization: plain text preserved; HTML escaped; emojis/newlines round-trip.\n- TimezoneOffset: capture offset accuracy and consistent local render.\n\n

### Integration
POST /rides success: 201 with id, durationMinutes; RideRecorded persists; projection updates.
\n- POST /rides validation: 400 field-specific errors (missing date/start/distance; ranges; future).\n- DB constraints: CHECK for distance/time; FK userId; NOT NULL for required.\n- Observability: correlated trace + structured logs; Insights exporter receives spans.\n\n### Contract\n- RideRecorded event schema: versioned; backward-compatible; required fields enforced; unknown ignored.\n- Projection schema: recent rides read model shape compatibility.\n\n### E2E (Playwright MCP)\n- Add ride flow: fill, submit, success, recent activity shows ride with duration and local times.\n- Duplicate guard: submit disabled during request prevents double submits.\n- Validation UX: inline errors appear/clear; inputs preserved on error.\n- Accessibility/responsive: keyboard navigation; mobile/tablet/desktop breakpoints.\n\n### Performance\n- API latency: p95 < 500ms for POST /rides under light load.\n- Projection lag: read model reflects new ride within ≤ 5s.\n