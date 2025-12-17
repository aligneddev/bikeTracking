# Bike Ride Tracking System

A comprehensive bike ride tracking application built with .NET 10, Blazor WebAssembly, and Azure Aspire. Track your rides with weather data, edit ride details, and view comprehensive ride history.

Built as an example of using [SpecKit](https://www.github.com/spec-kit).

## Features

✅ **Ride Management**
- Record rides with date, hour, distance, and locations
- Automatic weather data capture from NOAA
- Edit ride details with weather re-fetch
- View comprehensive ride details with weather conditions

✅ **Technical Highlights**
- Event Sourcing architecture with full audit trail
- CQRS pattern with read/write model separation
- Clean Architecture with Domain-Driven Design
- OAuth authentication
- Graceful degradation for weather API failures
- Fluent UI Blazor components with custom branding

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Visual Studio 2025](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- SQL Server LocalDB or Azure SQL Database
- [NOAA API Token](https://www.ncdc.noaa.gov/cdo-web/token) (free)

## Quick Start

### 1. Clone the Repository

```bash
git clone <repository-url>
cd bikeTracking
```

### 2. Configure Settings

Create or update `bikeTracking.ApiService/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "BikeTrackingDb": "Server=(localdb)\\mssqllocaldb;Database=BikeTracking;Trusted_Connection=True;"
  },
  "WeatherService": {
    "ApiToken": "YOUR_NOAA_API_TOKEN_HERE",
    "NoaaBaseUrl": "https://api.weather.gov"
  },
  "Authentication": {
    "Authority": "https://login.microsoftonline.com/YOUR_TENANT_ID",
    "ClientId": "YOUR_CLIENT_ID",
    "ClientSecret": "YOUR_CLIENT_SECRET"
  }
}
```

### 3. Run Database Migrations

```bash
cd BikeTracking.Infrastructure
dotnet ef database update --context BikeTrackingContext
```

### 4. Start the Application

```bash
cd ..
dotnet run --project bikeTracking.AppHost
```

The application will be available at:
- **Frontend**: https://localhost:5000
- **API**: https://localhost:5001
- **Swagger**: https://localhost:5001/swagger

## Project Structure

```
bikeTracking/
├── bikeTracking.AppHost/           # Aspire orchestration
├── bikeTracking.ApiService/        # Minimal API backend
├── bikeTracking.WebWasm/           # Blazor WebAssembly frontend
├── BikeTracking.Domain/            # Domain entities, events, commands
├── BikeTracking.Infrastructure/    # EF Core, repositories, services
├── BikeTracking.Shared/            # DTOs and contracts
├── bikeTracking.ServiceDefaults/   # Shared service configuration
├── bikeTracking.Tests/             # Unit and integration tests
└── specs/                          # Feature specifications
    └── 001-ride-tracking/          # Current spec documentation
```

## Architecture

### Event Sourcing
All ride changes (create, edit, delete) generate immutable domain events stored in the `Events` table, providing a complete audit trail.

### CQRS
- **Write Model**: Command handlers process requests and generate events
- **Read Model**: Projections optimize queries for listing and viewing rides

### Clean Architecture Layers
1. **Domain**: Pure business logic, no dependencies
2. **Infrastructure**: Database, external APIs, repositories
3. **API**: HTTP endpoints, authentication, middleware
4. **UI**: Blazor components, forms, validation

## Key Technologies

- **.NET 10**: Latest .NET framework
- **Blazor WebAssembly**: Client-side SPA framework
- **Aspire**: Cloud-native orchestration
- **EF Core**: ORM and database migrations
- **Fluent UI Blazor**: Microsoft Fluent Design components
- **NOAA Weather API**: Historical weather data
- **OAuth/OIDC**: Secure authentication
- **Azure Application Insights**: Telemetry and monitoring

## Development

### Building the Solution

```bash
dotnet build
```

### Running Tests

```bash
dotnet test
```

### Database Migrations

Create a new migration:
```bash
cd BikeTracking.Infrastructure
dotnet ef migrations add MigrationName --context BikeTrackingContext
```
 --connection "Server=127.0.0.1,1433;User ID=sa;Password

Apply migrations:
```bash
dotnet ef database update --context BikeTrackingContext
```

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/rides` | Create a new ride |
| GET | `/api/rides` | List user'\''s rides (paginated) |
| GET | `/api/rides/{id}` | Get ride details |
| PUT | `/api/rides/{id}` | Update ride |
| DELETE | `/api/rides/{id}` | Delete ride (≤90 days) |

See [API Documentation](specs/001-ride-tracking/contracts/README.md) for detailed contract specifications.

## Configuration

### Weather Service
The application uses NOAA'\''s free weather API. Get your token at https://www.ncdc.noaa.gov/cdo-web/token

### Authentication
Configure OAuth/OIDC with Microsoft Entra ID (Azure AD) or another OIDC provider.

### Database
Supports SQL Server, Azure SQL, or LocalDB. Update the connection string in `appsettings.Development.json`.

## Troubleshooting

### Weather Data Unavailable
The app gracefully handles weather API failures. Rides save successfully even if weather data is unavailable.

### Migration Errors
Ensure LocalDB is running: `sqllocaldb info mssqllocaldb`

### Build Warnings
Package vulnerability warnings for `Microsoft.Identity.*` packages are known issues. Update packages when patches are available.

## Future Features

The following features are planned for future releases (see spec `002-data-management-and-community`):
- Individual ride deletion with 90-day constraint enforcement
- GDPR-compliant data export and deletion requests
- Anonymous community statistics and leaderboards
- Social features (shareable rides, challenges)

## TODOs
- Figure out real Auth with EntraID and OAuth

## Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/my-feature`
3. Commit changes: `git commit -am "Add new feature"`
4. Push to branch: `git push origin feature/my-feature`
5. Submit a pull request

## License

See [LICENSE.txt](LICENSE.txt) for details.

## Documentation

- [Specification](specs/001-ride-tracking/spec.md): Feature requirements
- [Implementation Plan](specs/001-ride-tracking/plan.md): Technical architecture
- [Data Model](specs/001-ride-tracking/data-model.md): Entity relationships
- [Quick Start Guide](specs/001-ride-tracking/quickstart.md): Detailed setup instructions
- [Tasks](specs/001-ride-tracking/tasks.md): Implementation task breakdown

---

Built with ❤️ using .NET 10 and Azure Aspire
