## Quickstart

- Open specs/001-add-ride/contracts/openapi.json to review the POST /rides contract.
- Implement Blazor form RideRecorder with DataAnnotations and client-side submit disable during API call.
- Implement Minimal API endpoint POST /rides applying DataAnnotations on DTOs; emit RideRecorded event, persist to event store, update projection.
- Store timestamps in UTC; capture timezone offset; compute duration for response.
- Configure Aspire with OpenTelemetry; export to Application Insights locally and verify traces/logs/metrics for create operation.