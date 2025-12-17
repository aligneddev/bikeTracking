# API Contracts: Bike Ride Tracking

**Feature**: 001-ride-tracking  
**Base URL**: /api  
**Authentication**: OAuth 2.0 (Bearer token required for all endpoints except health)

---

## Endpoints

### Ride Management

#### POST /rides
Create a new ride

**Request**:
```json
{
  "date": "2025-12-10",
  "hour": 14,
  "distance": 25.5,
  "distanceUnit": "miles",
  "rideName": "Morning Commute",
  "startLocation": "Home",
  "endLocation": "Office",
  "notes": "Great weather today"
}
```

**Response** (201 Created):
```json
{
  "rideId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userId": "user123",
  "date": "2025-12-10",
  "hour": 14,
  "distance": 25.5,
  "distanceUnit": "miles",
  "rideName": "Morning Commute",
  "startLocation": "Home",
  "endLocation": "Office",
  "notes": "Great weather today",
  "weather": {
    "temperature": 72.5,
    "conditions": "Sunny",
    "windSpeed": 5.2,
    "windDirection": "NW",
    "humidity": 45.0,
    "pressure": 1013.25,
    "capturedAt": "2025-12-15T10:30:00Z"
  },
  "createdAt": "2025-12-15T10:30:00Z",
  "modifiedAt": null,
  "deletionStatus": "active",
  "ageInDays": 5
}
```

---

#### GET /rides
Get all rides for authenticated user

**Query Parameters**:
- page (int, default 1)
- pageSize (int, default 20, max 100)
- sortBy (string, default "createdAt")
- sortOrder (string, "asc" | "desc", default "desc")

**Response** (200 OK):
```json
{
  "rides": [
    {
      "rideId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "date": "2025-12-10",
      "rideName": "Morning Commute",
      "startLocation": "Home",
      "endLocation": "Office",
      "distance": 25.5,
      "distanceUnit": "miles",
      "ageInDays": 5
    }
  ],
  "totalCount": 42,
  "page": 1,
  "pageSize": 20
}
```

---

#### GET /rides/{rideId}
Get single ride details

**Response** (200 OK):
```json
{
  "rideId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userId": "user123",
  "date": "2025-12-10",
  "hour": 14,
  "distance": 25.5,
  "distanceUnit": "miles",
  "rideName": "Morning Commute",
  "startLocation": "Home",
  "endLocation": "Office",
  "notes": "Great weather today",
  "weather": {
    "temperature": 72.5,
    "conditions": "Sunny"
  },
  "createdAt": "2025-12-15T10:30:00Z",
  "modifiedAt": null,
  "deletionStatus": "active",
  "ageInDays": 5
}
```

---

#### PUT /rides/{rideId}
Update existing ride

**Request**:
```json
{
  "date": "2025-12-10",
  "hour": 15,
  "distance": 26.0,
  "distanceUnit": "miles",
  "rideName": "Afternoon Commute",
  "startLocation": "Home",
  "endLocation": "Office",
  "notes": "Updated notes"
}
```

**Response** (200 OK): Same as GET /rides/{rideId}

---

#### DELETE /rides/{rideId}
Delete ride (only if ageInDays ≤ 90)

**Response** (204 No Content)

**Error** (403 Forbidden) if ride > 90 days:
```json
{
  "error": "RideTooOld",
  "message": "Rides older than 90 days cannot be deleted via UI. Submit a formal deletion request.",
  "rideAge": 120
}
```

---

### User Preferences

#### GET /user/preferences
Get user preferences

**Response** (200 OK):
```json
{
  "userId": "user123",
  "distanceUnit": "miles",
  "communityOptIn": false,
  "createdAt": "2025-11-01T08:00:00Z",
  "modifiedAt": "2025-12-01T10:00:00Z"
}
```

---

#### POST /user/preferences
Update user preferences

**Request**:
```json
{
  "distanceUnit": "kilometers",
  "communityOptIn": true
}
```

**Response** (200 OK): Same as GET /user/preferences

---

### Data Management

#### POST /user/data-export
Request data export

**Response** (202 Accepted):
```json
{
  "requestId": "7fa85f64-1234-5678-b3fc-2c963f66afa6",
  "status": "pending",
  "estimatedCompletionTime": "2025-12-16T10:00:00Z"
}
```

---

#### GET /user/data-export/{requestId}
Check export status

**Response** (200 OK):
```json
{
  "requestId": "7fa85f64-1234-5678-b3fc-2c963f66afa6",
  "status": "completed",
  "downloadUrl": "https://storage.azure.com/exports/user123.zip",
  "expiresAt": "2025-12-18T10:00:00Z"
}
```

---

#### POST /user/data-deletion
Request data deletion (for rides > 90 days)

**Request**:
```json
{
  "scope": "older_than_3_months"
}
```

**Response** (202 Accepted):
```json
{
  "requestId": "8fa85f64-5678-1234-b3fc-2c963f66afa6",
  "status": "pending",
  "message": "Your deletion request will be processed within 30 days."
}
```

---

### Community Features

#### GET /community/statistics
Get anonymous community statistics

**Response** (200 OK):
```json
{
  "totalRides": 15420,
  "totalDistance": 385500.5,
  "averageDistance": 25.0,
  "rideFrequencyTrends": {
    "2025-11": 5200,
    "2025-12": 5100
  },
  "lastUpdated": "2025-12-15T02:00:00Z"
}
```

---

#### GET /community/leaderboards
Get anonymized leaderboards

**Query Parameters**:
- metric (string, "distance" | "frequency", default "distance")
- limit (int, default 100, max 100)

**Response** (200 OK):
```json
{
  "metric": "distance",
  "leaderboard": [
    {
      "rank": 1,
      "userId": "anon-abc123",
      "value": 2500.5
    },
    {
      "rank": 2,
      "userId": "anon-def456",
      "value": 2300.0
    }
  ],
  "lastUpdated": "2025-12-15T02:00:00Z"
}
```

---

## Error Responses

### 400 Bad Request
```json
{
  "error": "ValidationError",
  "message": "Invalid input",
  "details": {
    "date": ["Date must be within the last 90 days"],
    "hour": ["Hour must be between 0 and 23"]
  }
}
```

### 401 Unauthorized
```json
{
  "error": "Unauthorized",
  "message": "Valid authentication token required"
}
```

### 403 Forbidden
```json
{
  "error": "Forbidden",
  "message": "You do not have permission to access this resource"
}
```

### 404 Not Found
```json
{
  "error": "NotFound",
  "message": "Ride not found"
}
```

### 500 Internal Server Error
```json
{
  "error": "InternalServerError",
  "message": "An unexpected error occurred"
}
```

---

**Phase 1: API Contracts Complete** ✅
