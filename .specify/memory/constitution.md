# Bike Tracking Application Constitution
<!-- Sync Impact Report v1.1.1 → v1.2.0
Version Change: MINOR (1.2.0)
Rationale: Added new Principle VII (Data Validation ;& Integrity) establishing DataAnnotationsAttributes validation discipline across frontend and backend. Added data naming conventions (SAMPLE_/DEMO_ prefixes) and approved Playwright MCP for E2E testing. Materially expands governance for data quality without changing existing principles.
Modified Sections:
- Added Principle VII: Data Validation & Integrity (client-side, server-side, database constraints)
- Development Workflow: Added "Data Naming Conventions" subsection with SAMPLE_/DEMO_ prefix rules
- Approved MCP Tools: Added new "Testing & Quality Assurance" category with Playwright MCP
- Governance: Updated Compliance Review to include data validation audits
Status: All changes approved by user; no deferred items
Previous Updates:
- v1.1.1 (2025-12-15): Added Entity Framework Core specification, EF Core testing strategy
- v1.1.0 (2025-12-11): Added Test Plan Phase, NuGet Package Discipline, and 3 MCP tools (monitor, resourcehealth, role)
- v1.0.0 (2025-12-11): Initial ratification with 6 core principles and 14 MCP tools
-->

## Core Principles

### I. Clean Architecture & Domain-Driven Design

Domain logic isolated from infrastructure concerns via layered architecture aligned with Biker Commuter aggregates: Rides, Expenses, Savings Calculations. Infrastructure dependencies (database, HTTP clients, external APIs) must be injectable and independently testable. Use domain models to express business rules explicitly; repositories and services should abstract data access. Repository pattern separates domain models from persistence details.

**Rationale**: Testability without mocking infrastructure; business logic remains framework-agnostic and reusable; easier to reason about domain behavior independent of deployment environment.

### II. Functional Programming (Pure & Impure Sandwich)

Core calculations and business logic implemented as pure functions: distance-to-distance conversions, expense-to-savings transformations, weather-to-recommendation mappings. Pure functions have no side effects—given the same input, always return the same output. Impure edges (database reads/writes, external API calls, user input, system time) explicitly isolated at application boundaries. Handlers orchestrate pure logic within impure I/O boundaries.

**Rationale**: Pure functions are trivially testable, deterministic, and composable. Side effect isolation makes dataflow explicit and reduces debugging complexity. Immutable data structures preferred where practical.

### III. Event Sourcing & CQRS

Every domain action (ride recorded, expense added, savings recalculated) generates an immutable, append-only event stored in the event store. Commands transform to events; events drive projections (read models). Current state always derived from event history. Write and read models separated: writes append events; reads query projections (materialized views). Change Event Streaming (CES) triggers background functions to build read-only projections asynchronously.

**Rationale**: Complete audit trail guaranteed; temporal queries enabled; event replays support debugging and future features; projections scale independently of event volume; data consistency enforced via event contracts.

### IV. Quality-First Development (Test-Driven)

Red-Green-Refactor cycle is **non-negotiable**: tests written first, approved by user, then fail, then implementation follows. Unit tests validate pure logic (target 85%+ coverage). Integration tests verify each vertical slice end-to-end. Contract tests ensure event schemas remain backwards compatible. Security tests validate OAuth isolation and data access. **Agent must suggest tests with rationale; user approval required before implementation.**

**Rationale**: Tests act as executable specifications; catches bugs early; refactoring confidence; documents intended behavior; prevents regressions.

### V. User Experience Consistency & Accessibility

All frontend UI built with Fluent UI Blazor (v4.13.2) using design tokens derived from brand palette (FFCDA4, FFB170, FF7400, D96200, A74C00). Centralized DesignTheme component enforces visual consistency. WCAG 2.1 AA compliance mandatory (semantic HTML, color contrast, keyboard navigation, screen reader support). Mobile-first responsive design (breakpoints: mobile ≤600px, tablet 601-1024px, desktop >1024px). OAuth identity integration ensures users access only their own data; public data (leaderboards, shared rides) clearly marked. Simple, intuitive UX; avoid feature creep.

**Rationale**: Brand consistency builds trust; accessibility ensures inclusive product; responsive design reaches all devices; identity isolation ensures privacy compliance; simplicity reduces cognitive load and maintenance burden.

### VI. Performance, Scalability & Observability

API response times must remain **<500ms at p95** under normal load; database indexes optimized for event queries. Static assets served via CDN. Background Azure Functions process Change Event Streaming events asynchronously to build read projections; acceptable lag is eventual consistency within 5 seconds. Structured logging (JSON) to Application Insights for all API requests, domain events, and errors. Metrics tracked: API latency, event processing lag, error rates, user engagement. Aspire orchestration enables local debugging; Azure Container Apps provides production scalability via Managed Identity and VNet integration.

**Rationale**: Sub-500ms response ensures fluid UX; scalable projections decouple write and read performance; structured observability enables rapid incident response; Container Apps autoscaling handles demand spikes.

### VII. Data Validation & Integrity

All user input **MUST** be validated in three layers: (1) **Client-side (Blazor)** using Microsoft DataAnnotationsAttributes for immediate feedback and UX responsiveness; (2) **Server-side (Minimal API)** using DataAnnotationsAttributes with attribute-based validation on command/event DTOs; (3) **Database layer** via constraints (NOT NULL, UNIQUE, FOREIGN KEY, CHECK). Validation rules enforced consistently across frontend and backend—if a field is required in Blazor form, the API endpoint MUST also enforce that constraint via data annotations. No data enters the system without validation. Referenced documentation: https://learn.microsoft.com/en-us/aspnet/core/blazor/forms/validation

**Rationale**: Defense-in-depth prevents invalid data from corrupting event store or projections; client-side validation improves UX responsiveness; server-side validation prevents bypass attacks; database constraints provide last-line guarantees. Combined approach ensures data integrity without redundant checks.

## Technology Stack Requirements

### Backend & Orchestration
- **Framework**: .NET 10 Minimal API (latest stable)
- **Orchestration**: Microsoft Aspire (latest stable) for local and cloud development
- **Language**: C# (latest language features: records, pattern matching, async/await)
- **NuGet Discipline**: All packages must be checked monthly for updates; security patches applied immediately; major versions reviewed for breaking changes before upgrade

### Frontend
- **Framework**: Microsoft Blazor .NET 10 (latest stable)
- **UI Library**: Fluent UI Blazor v4.13.2 (or latest patch within v4.13.x)
- **Authentication**: OAuth (via Microsoft.AspNetCore.Authentication.OpenIdConnect)
- **Design System**: Centralized DesignTheme with design tokens; theme colors locked to brand palette
- **Validation**: Microsoft DataAnnotationsAttributes for form validation (see Principle VII)

### Data & Persistence
- **Primary Database**: Azure SQL Database (serverless elastic pools in production)
- **ORM & Data Access**: Entity Framework Core (latest .NET 10 compatible version) for all database interactions; DbContext per aggregate root; repositories abstract EF Core from domain layer
- **Schema Management**: EF Core migrations for code-first schema evolution
- **Event Store**: Dedicated event table (Events with columns: EventId, AggregateId, EventType, Data JSON, Timestamp, Version); events stored as JSON via EF Core value converters
- **Read Projections**: Separate read-only tables (e.g., RideProjection, SavingsProjection) built by background functions; queried via dedicated read-only DbContext
- **Change Event Streaming**: Azure SQL Change Tracking or Change Data Capture for triggering Azure Functions

### Infrastructure & DevOps
- **Hosting**: Azure Container Apps (managed Kubernetes) for API and Blazor frontend
- **Identity**: Azure Managed Identity for service-to-service authentication; no connection strings in code
- **Secrets Management**: Azure Key Vault for database credentials, API keys, OAuth secrets
- **Logging & Monitoring**: Application Insights for structured logging, metrics, and distributed tracing
- **CI/CD**: GitHub Actions with Aspire and azd (Azure Developer CLI) for orchestrated deployment
- **Deployment Artifacts**: IaC via Bicep (Azure Resource Manager templates)

### Package Management & Updates
- Check latest NuGet versions monthly; update patches for security; propose major/minor upgrades with test coverage
- Pin versions explicitly in .csproj or Directory.Packages.props
- Use mcp_nuget_get-latest-package-version to verify package status before implementation

## Development Workflow

### Specification & Vertical Slices
Each specification defines a **complete, deployable vertical slice**:
- **Frontend**: Blazor page + reusable components with DesignTheme styling
- **API**: One or more Minimal API endpoints handling commands/queries
- **Database**: Event table, read projection table, SQL migrations via .sqlproj
- **Integration**: Background function or event handler to materialize projections (if applicable)
- **Deployment**: Tested locally via Aspire, deployable to Azure Container Apps

Example: "User records a bike ride" slice includes:
- Blazor form component (RideRecorder.razor, styled with DesignTheme, with DataAnnotationsAttributes validation)
- POST /rides API endpoint (command handler with DataAnnotationsAttributes on DTO)
- Events table with RideRecorded event; Projections table (RideProjection)
- Background function listening to CES to update RideProjection
- Aspire host configuration; Bicep template for Container Apps

### Data Naming Conventions

All data created during development **MUST** follow strict naming conventions to ensure test data is never deployed to production:

- **SAMPLE_**: Prefix all representative, realistic sample data (e.g., SAMPLE_Ride_CoastalCommute, SAMPLE_User_AlexJones). Sample data demonstrates expected data shapes and is used for documentation and demos.
- **DEMO_**: Prefix all dummy, placeholder, or throwaway test data (e.g., DEMO_Ride_12345, DEMO_ExpenseTemp). Demo data is strictly for local development and testing; never deployed to production.
- **Production data**: Real user data without prefixes. No prefixes used for live, user-entered data.

**Agent must ask user approval before creating ANY data** (sample, demo, or production fixtures). User specifies: is this sample data for docs? Demo for testing? A fixture for a test scenario? Agent confirms naming prefix and purpose before generation.

**Rationale**: Clear naming prevents accidental production deployments of test data; naming convention facilitates automated cleanup of test data; explicit approval ensures data creation aligns with specification intent.

### Testing Strategy (User Approval Required)

Tests suggested by agent must receive explicit user approval before implementation. Test categories by slice:

**Unit Tests** (pure logic, 85%+ target coverage)
- Event serialization/deserialization
- Validation rules (including DataAnnotationsAttributes behavior)
- Pure function calculations

**Integration Tests** (end-to-end slice verification)
- OAuth token validation → data isolation enforced
- Database migrations run successfully
- Entity Framework DbContext configuration validated
- Validation attributes enforced on API endpoints

**Contract Tests** (event schema stability)
- Event schema versioning
- Backwards compatibility of event handlers
- Projection schema changes

**Security Tests**
- OAuth token required for all user endpoints
- User can only access their own data
- Anonymous access to public data (if applicable)
- Data validation prevents injection attacks

**Database Tests**
- Migration up/down transitions
- Event table constraints (unique EventId, non-null fields)
- Foreign key integrity for aggregates
- DataAnnotations constraints validated at database layer

**E2E Tests** (critical user journeys using Playwright MCP)
- Complete user workflows from frontend form submission to event storage to projection materialization
- Form validation feedback displayed correctly
- Data persisted and visible in read projections
- Responsive design verified across breakpoints

**Performance Tests** (under acceptance criteria)
- Projection lag <5 seconds after event insertion
- API endpoints meet <500ms p95 response time SLO under load

### Development Approval Gates

1. **Specification Approved**: Spec document completed and user-approved before coding
2. **Tests Approved**: Agent proposes tests; user approves test plan
3. **Tests Fail (Red)**: Proposed tests run and fail without implementation
4. **Implementation (Green)**: Code written to make tests pass
5. **Refactor (Refactor)**: Code cleaned, tests still pass
6. **Code Review**: Implementation reviewed for architecture compliance, naming, performance, validation discipline
7. **Local Deployment**: Slice deployed locally via Aspire, tested manually with Playwright if E2E slice
8. **Azure Deployment**: Slice deployed to Azure Container Apps via GitHub Actions + azd
9. **User Acceptance**: User validates slice meets specification and data validation rules observed

## Approved MCP Tools

### Documentation & Learning
- **mcp_microsoftdocs_microsoft_docs_search** – Search Microsoft Learn for .NET, Blazor, Azure documentation
- **mcp_microsoftdocs_microsoft_code_sample_search** – Retrieve official C# and .NET code samples
- **mcp_microsoftdocs_microsoft_docs_fetch** – Fetch full documentation pages for detailed guidance (tutorials, prerequisites, troubleshooting)

### Azure Services & Infrastructure
- **mcp_microsoft_azu2_documentation** – Azure-specific guidance and best practices
- **mcp_microsoft_azu2_deploy** – Deployment planning, architecture diagram generation, IaC rules, app logs
- **mcp_microsoft_azu2_extension_cli_generate** – Generate Azure CLI commands for infrastructure setup
- **mcp_microsoft_azu2_azd** – Azure Developer CLI for Aspire orchestration and deployment
- **mcp_microsoft_azu2_appservice** – Manage Azure App Service resources (if used for preview deployments)
- **mcp_microsoft_azu2_sql** – Azure SQL operations: list databases, execute queries, manage servers
- **mcp_microsoft_azu2_storage** – Storage account management for CDN, static assets
- **mcp_microsoft_azu2_keyvault** – Azure Key Vault secrets and certificate management
- **mcp_microsoft_azu2_get_bestpractices** – Azure best practices for code generation, operations, and deployment

### Package & Dependency Management
- **mcp_nuget_get-latest-package-version** – Check for latest NuGet package versions
- **mcp_nuget_get-package-readme** – Retrieve package documentation and usage examples
- **mcp_nuget_update-package-to-version** – Update packages to specific versions with dependency resolution

### Testing & Quality Assurance
- **Playwright MCP** – End-to-end browser automation for critical user journeys; write and run Playwright test code for UI validation, form submission, and responsive design verification

### Source Control & Examples
- **github_repo** – Search GitHub repositories for code examples and patterns (e.g., Event Sourcing with .NET, Blazor authentication)

## Governance

### Constitution as Governing Document
This constitution supersedes all other project guidance. All architectural decisions, code reviews, deployment approvals, and spec acceptance gates must verify compliance with these seven core principles and technology stack requirements.

### Amendment Procedure
Amendments must:
1. Document rationale for change
2. Propose new or modified principle(s) with concrete examples
3. Identify affected specifications and templates (plan, spec, tasks, commands)
4. Include migration plan for in-flight work
5. Receive user approval before ratification

Version bumping:
- **MAJOR**: Principle removal or redefinition (e.g., removing Event Sourcing, changing auth model)
- **MINOR**: New principle, new technology stack component, or major section expansion (e.g., adding data validation requirements, new MCP tool category)
- **PATCH**: Clarifications, wording refinements, typo fixes, example updates (no semantic change to governance)

### Compliance Review
- Weekly: Code reviews verify architecture compliance (Clean Architecture layers, pure/impure separation, event semantics, validation discipline)
- Per-spec: Testing strategy approved before implementation; vertical slice completeness validated; data naming conventions observed
- Monthly: Technology stack checked for security patches and major updates; NuGet packages reviewed; data validation audit (sample/demo data cleaned up before production deployments)

### Template Alignment
All SpecKit templates must reflect this constitution:
- **.specify/templates/plan-template.md**: Incorporate constitution principles into success criteria; include data naming conventions
- **.specify/templates/spec-template.md**: Mandate event sourcing schemas, testing categories, acceptance criteria, validation requirements
- **.specify/templates/tasks-template.md**: Align task types with principles (e.g., "Event Handler", "Projection", "Integration Test", "Security Audit", "Playwright E2E Test")
- **.specify/templates/commands/*.md**: Reference this constitution for guidance; agent-specific names (e.g., "Copilot") replaced with generic guidance

### Runtime Guidance
Development workflow guidance documented in [README.md](../../README.md) and .github/prompts/ directory. This constitution establishes governance; runtime prompts add context and tool references.

Always commit before continuing to a new phase.

---

**Version**: 1.2.0 | **Ratified**: 2025-12-11 | **Last Amended**: 2025-12-15