# Bike Ride Tracking System

A comprehensive bike ride tracking application built with .NET 10, Blazor WebAssembly, and Azure Aspire. Track your rides with weather data, edit ride details, and view comprehensive ride history.

Created with SpecKit and Specification Driven Development (SDD) principles to demonstrate the using SpecKit. I will start with a PowerPoint presentation. TODO: add link to presentation.

https://github.com/github/spec-kit/blob/main/spec-driven.md#core-principles


## Starting Point

I created the `startingPoint-preConstitution` branch with `aspire new` chosing the Blazor and Minimal API for Aspire 13.0.2 on 12/11/2025.

I then setup SpecKit toolkit with `uv tool install specify-cli --from git+https://github.com/github/spec-kit.git`
Then used `specify init .` selecting Copilot and Powershell, to add the SpecKit files to the project.

You can see the instructions in the repo here: https://github.com/github/spec-kit

`speckit check` does not list Visual Studio, so I'll work in VS Code.

You'll need Docker or Podman to run the containers that Aspire creates.

## 📋 Project Governance

**All development is governed by the [Bike Tracking Application Constitution](.specify/memory/constitution.md).**

### SpecKit

Use SpecKit to create specifications and run the development. It is essential to keep the specifications up to date as the source of truth for the project so we can follow the SDD principles.

#### Constitution

This is the constitution prompt I used:

```markdown
Create with principles focused on code quality, testing standards, user experience consistency, and performance requirements following Clean Architecture, Functional Programming (pure and impure function sandwich, use Results instead of Exceptions, avoid using Exceptions), Event Sourcing and Domain Driven Development ideas to create high scalable, quality and usable web application and https API and an SQL Database. 
Suggest tests, but ask for my input before creating tests.

Focus on creating a working vertical slice of functionality for each specification. We value working software after running /speckit.implementation task. Create end-to-end user flow UI tests using Playwright code and MCP when a end-to-end user flow is complete.

Use the MCP tools for MS Learn (for information), GitHub (source control and actions), Azure MCP for gathering information. Use the Playwright MCP. Suggest other  MCP tools to use and record those in this constitution. Prompt me for permission to use these. If you are unsure, use a web search and MS Learn to make sure your information is up to date.

We will use the latest Aspire orchestration, latest C# features, C# with .Net 10 Minimal API for the API backend. Make sure we have the latest NuGet packages and ask to update when you see any out of date. Do not add any packages with my permission.

Microsoft Blazor .Net 10 for the front end (with responsive design and and simple UX. The user will login with an OAUTH identity (look up information in MS Learn https://learn.microsoft.com/en-us/aspnet/core/blazor/security/?view=aspnetcore-10). The user will have access to only their data and publicly available (use the latest Blazor FluentUI v4.13.2 https://www.fluentui-blazor.net/, componentize with "Design Tokens", create a DesignTheme using these colors for a palette "FFCDA4, FFB170, FF7400, D96200, A74C00" to follow our branding)).

Validate all data in the front and back end using the Microsoft DataAnnotationsAttributes https://learn.microsoft.com/en-us/aspnet/core/blazor/forms/validation?view=aspnetcore-10.0&preserve-view=true

The database will be an Azure SQL database. We will use the new SDK style database project to handle all database changes. Use the latest Entity Framework Core. Following Event Sourcing, create tables to store each event. Current projections will be created in a background Azure Function listening for Change Event Streaming Events (CES) and stored in a different read only table.
 
Prefix any sample data with SAMPLE_. Prefix any dummy/demo data with DEMO_. Ask before creating data.
 
The application will be hosted in Azure.  With Aspire, host the application in Azure Container Apps. Use Managed Identity. All secrets must be in the Azure Key Vault.

DevOps: Pipelines will be for GitHub Actions using the Aspire and `azd` tooling to deploy. Create templates actions for easier reuse.

Git: Always commit before continuing to a new phase.
```

### Business Rules/Features

We are creating a new product called Biker Commuter. We want to enable users to quickly and easily track their bike rides. 
They will use this data to track the savings instead of driving, see the historical weather and use this data for deciding on what to wear on a ride, give motivation to ride more and possible share with others.  

It will track the distance, time, current weather, expenses, gas prices, gallons of gas saved, and Co2 saved. 
It will also give an estimate of the total savings based on the mileage rate and a different savings based on average gas prices and vehicle miles per gallon. 
The product will also have a feature to track the number of rides and the average distance per ride. 
We will show some charts and graphs to visualize the miles and savings. 
The user will be able to see these for the current year, the total for all the years and be able to drill into each month or day.

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
