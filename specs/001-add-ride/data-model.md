## Entities

### Ride
- id: GUID
- userId: GUID
- rideDate: date (local date at entry)
- startTimeUtc: datetime (UTC)
- endTimeUtc: datetime (UTC)
- entryTimezoneOffsetMinutes: int
- distanceValue: decimal(6,1) miles
- notes: nvarchar(max)
- createdAt: datetime (UTC)
- Derived: duration = endTimeUtc - startTimeUtc; displayStart/End in user local time

### User
- userId: GUID
- displayName: string (read-only)

### Validation Rules
- Required: rideDate, startTimeUtc, distanceValue
- Distance: 0.1 <= miles <= 1000.0; precision ≥ 0.1
- Time: endTimeUtc >= startTimeUtc; no future times at submission
- Notes: plain text; HTML escaped; emojis/newlines allowed
