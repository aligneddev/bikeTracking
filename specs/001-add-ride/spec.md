# Feature Specification: Add Ride Entry

**Feature Branch**: `001-add-ride`  
**Created**: 2025-12-16  
**Status**: Draft  
**Input**: User description: "The user (tied to his logged in identity) will be able to add a ride with a date, time, distance, and notes."

## Clarifications

### Session 2025-12-16
- Q: Authentication Approach → A: Fake/stub authentication for MVP; real auth deferred.
- Q: Notes Formatting Policy → A: Plain text only; escape HTML; emojis and newlines preserved.
- Q: Observability Signals → A: Full OpenTelemetry via Aspire on client and server (traces, metrics, structured logs) with W3C trace context.
- Q: Date/Time Storage & Display → A: Store timestamps in UTC; capture user timezone offset at entry; display in user’s current local time.
- Q: Past Date Limit → A: Allow any past date (no maximum lookback).
- Q: Duplicate Submission Handling → A: Client-only: disable submit until response.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Add a ride (Priority: P1)

An authenticated user records a completed bike ride by entering the ride date, start time, end time (defaulting to now), distance (in miles), and optional notes, then saves it.

**Why this priority**: Core value of the app is tracking rides; without this, the product lacks purpose.

**Independent Test**: A user with an active session can add a ride with valid inputs and receives a clear success confirmation; the ride appears in their recent activity.

**Acceptance Scenarios**:

1. **Given** an authenticated user and valid inputs (date, start time, end time, distance), **When** the user saves, **Then** the ride is created, associated to the user, and a success message displays.
2. **Given** a newly created ride, **When** viewing recent rides, **Then** the new ride is visible with entered fields and computed duration (end minus start).

---

### User Story 2 - Input validation and feedback (Priority: P2)

The system validates required fields and provides inline error messages so users can correct mistakes without losing entered data.

**Why this priority**: Prevents bad data and reduces user frustration.

**Independent Test**: Submit with missing or invalid fields and verify specific, actionable error messages; after correction, saving succeeds.

**Acceptance Scenarios**:

1. **Given** missing required fields (date, start time, distance), **When** the user submits, **Then** the system shows field-specific errors and prevents save.
2. **Given** an end time earlier than start time, **When** the user submits, **Then** the system shows a validation error to correct the times.
3. **Given** a distance outside allowed range, **When** the user submits, **Then** the system shows a validation error indicating allowed range.
4. **Given** a future-dated ride (date/time in the future), **When** the user submits, **Then** the system blocks save and indicates rides must be past or today.

---

### User Story 3 - Optional notes (Priority: P3)

The user can add optional free-text notes to a ride (e.g., conditions, route, effort) within a reasonable character limit.

**Why this priority**: Enhances usefulness but not critical for core tracking.

**Independent Test**: Add a ride with notes near the limit; saving succeeds and notes display correctly.

**Acceptance Scenarios**:

1. **Given** notes within the allowed length, **When** the user saves, **Then** the ride is created and notes are persisted.

### Edge Cases

- Date is today vs. past; end-of-month and leap day dates.
- Very old historical dates (e.g., decades ago) should be accepted.
- Start/end time near midnight; daylight saving transitions; ensure end ≥ start on the selected date.
- Very small distances (e.g., 0.1) and very large distances (e.g., 1000).
- Notes at maximum length; notes containing emojis and punctuation.
- Duplicate rapid submissions (double-click); prevent duplicate rides.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST allow authenticated users to create a ride entry with date, start time, end time, distance, and optional notes.
- **FR-002**: System MUST associate each ride to the currently logged-in user identity.
- **FR-003**: System MUST validate required fields (date, start time, distance) and block save with clear, field-specific error messages when invalid.
- **FR-004**: System MUST constrain distance to a reasonable range (default 0.1 to 1000.0) and support at least one decimal place precision.
- **FR-005**: System MUST treat notes as plain text only: escape HTML on input or render, preserve newlines, allow emojis; no Markdown or rich text in MVP.
- **FR-006**: System MUST display a success confirmation after a ride is saved and make the ride visible in the user's recent activity.
- **FR-007**: System MUST disable the submit action on the client while a save request is in-flight to prevent duplicate submissions. Server-side idempotency is out of scope for this MVP.
- **FR-008**: System SHOULD prefill date with the user's current local date and prefill end time to the user's current local time at the moment the form opens.
- **FR-009**: System MUST store ride start and end timestamps in UTC and capture the user's timezone offset at entry (e.g., minutes from UTC) for auditability and correct local rendering.
- **FR-010**: System MUST represent distance in miles by default; users cannot change the unit in this MVP. All displays use miles.
- **FR-011**: System MUST prohibit future-dated rides: the ride date/time cannot be in the future relative to submission; end time MUST be on or after start time and not in the future.
- **FR-012**: System MUST compute and display the ride duration based on start and end time.
- **FR-013**: System MUST allow any past date (no maximum historical lookback) for ride entries.
- **FR-014**: System MUST emit OpenTelemetry signals for ride creation on both client and server: correlated traces (W3C trace context), structured logs for validation errors, and a latency metric for create operations.

### Key Entities *(include if feature involves data)*

- **Ride**: Captures a single ride entry for a user. Attributes: id, userId, rideDate (local date selected), startTimeUtc, endTimeUtc, entryTimezoneOffsetMinutes, distanceValue (miles), notes, createdAt. Derived: duration (endTimeUtc − startTimeUtc), displayStart/End in user local time.
- **User**: Represents the authenticated account creating rides. Attributes: userId, displayName (read-only here); relationship: has many rides.

## Assumptions

- For MVP, authentication is faked/stubbed; a fixed test user is assumed. Real authentication is deferred and out-of-scope for this feature.

- The user is already authenticated; authentication and identity management are out of scope for this feature.
- Distance unit is miles for MVP; no per-user preference UI in this scope.
- Rides log completed activity only; future-dated entries are not allowed.
- Any past date is valid; no max lookback. Retention policies, if any, are out-of-scope for this feature.
- All timestamps are stored in UTC with the original offset captured; UI renders times in the user's current local time.
- No sharing/visibility controls are needed for this feature; rides are private to the user by default.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 95% of users can successfully add a ride on first attempt without needing support.
- **SC-002**: Users can add a ride in under 30 seconds from opening the form to confirmation with typical inputs.
- **SC-003**: Validation errors, when present, are specific and actionable; less than 2% of submissions result in unknown or generic errors.
- **SC-004**: After saving, the new ride appears in the user's recent activity immediately.






