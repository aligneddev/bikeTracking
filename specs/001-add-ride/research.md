## Research

- Decision: Use SQLite for local dev; Azure SQL for prod.
  - Rationale: Lightweight, zero-config locally; aligns with EF Core; easy to swap to Azure SQL via connection string.
  - Alternatives considered: Local SQL Server Express (heavier footprint); In-memory provider (not representative of production).

- Decision: OpenTelemetry via Aspire across client/server; Application Insights exporter.
  - Rationale: Constitution requires observability; Aspire simplifies local tracing; Insights provides managed backend.
  - Alternatives considered: Console logging only (insufficient), custom exporters (complex).

- Decision: DataAnnotations for validation on DTOs and Blazor forms; DB CHECK constraints for distance and timestamps.
  - Rationale: Principle VII mandates three-layer validation.
  - Alternatives considered: FluentValidation only (lacks DB guarantees).

- Decision: Event type RideRecorded with schema capturing UTC times, local date, timezone offset, distance, notes, userId.
  - Rationale: Auditable, drives projections; matches spec fields.
  - Alternatives considered: Direct row insert without events (violates ES/CQRS principle).
