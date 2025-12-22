# Implementation Tasks: Entra ID Authentication with MSAL

**Feature**: `002-entra-auth-msal` | **Spec**: [spec.md](spec.md) | **Plan**: [plan.md](plan.md)

**Feature Description**: Implement real Entra ID authentication using MSAL in both Blazor WebAssembly frontend and .NET 10 API backend. Replace test authentication handler with production-ready OAuth 2.0/OpenID Connect flow.

**Entra ID Configuration**:
- ClientID: `a9c55801-9ab4-45f9-a777-9320685f21ea`
- Object ID: `633fc7f8-8bea-4214-bdc6-76e4404b9cde`
- Directory ID: `612b54fd-5aad-4fcc-add3-0b203ac0d9e0`
- Callback URLs:
  - Login callback: `https://localhost:7265/authentication/login-callback`
  - Logout callback: `https://localhost:7265/authentication/logout-callback`

---

## Phase 1: Setup & Configuration

### Setup Phase - Prerequisites & Infrastructure

- [ ] T001 Review Entra ID app registration in Azure Portal and verify ClientID, Directory ID, and redirect URLs are correctly configured
- [ ] T002 [P] Add Microsoft.Identity.Web NuGet package to bikeTracking.ApiService project for backend token validation
- [ ] T003 [P] Add Microsoft.AspNetCore.Authentication.OpenIdConnect NuGet package to bikeTracking.ApiService
- [ ] T004 [P] Install MSAL.js packages in bikeTracking.WebWasm frontend (`@azure/msal-browser`, `@azure/msal-vue`)
- [ ] T005 [P] Install Pinia state management library in bikeTracking.WebWasm for authentication state management
- [ ] T006 Create appsettings.json configuration entries for Entra ID (Authority, ClientID, Audience) in bikeTracking.ApiService
- [ ] T007 Create appsettings.Development.json with localhost redirect URL override in bikeTracking.ApiService
- [ ] T008 Create .env.development file in bikeTracking.WebWasm with Entra ID client configuration for local development

---

## Phase 2: User Story 1 - Web UI User Authentication (P1)

### US1 Phase - Frontend Authentication with MSAL

**Story Goal**: Users can authenticate via Microsoft Entra ID and access protected pages; unauthenticated users see only main page

**Independent Test**: Open web UI → redirect to Microsoft login → authenticate → return to app with token → can access protected pages

**Test Strategy**: Manual browser testing + integration tests for login flow and protected page access

#### US1 - Unit Tests

- [ ] T009 [US1] Create unit test for AuthenticationService.LoginAsync() in bikeTracking.Tests/Authentication/AuthenticationServiceTests.cs
- [ ] T010 [US1] Create unit test for AuthenticationService.LogoutAsync() in bikeTracking.Tests/Authentication/AuthenticationServiceTests.cs
- [ ] T011 [US1] Create unit test for AuthenticationService.GetAccessTokenAsync() in bikeTracking.Tests/Authentication/AuthenticationServiceTests.cs
- [ ] T012 [US1] Create unit test for LoginButton.razor component rendering and click behavior in bikeTracking.Tests/Components/LoginButtonTests.cs
- [ ] T013 [US1] Create unit test for AuthorizeView.razor component logic in bikeTracking.Tests/Components/AuthorizeViewTests.cs

#### US1 - Frontend Implementation

- [ ] T014 [P] [US1] Create AuthenticationService.cs in bikeTracking.WebWasm/Services/ as wrapper around MSAL.js for login/logout/token acquisition via JavaScript interop
- [ ] T015 [US1] Create AuthenticationStateService.cs in bikeTracking.WebWasm/Services/ to provide Blazor AuthenticationStateProvider for app-wide authentication state
- [ ] T016 [US1] Create ApiClient.cs in bikeTracking.WebWasm/Services/ with Bearer token injection middleware for all HTTP requests
- [ ] T017 [P] [US1] Create LoginButton.razor component in bikeTracking.WebWasm/Components/ that triggers MSAL authentication flow
- [ ] T018 [P] [US1] Create LogoutButton.razor component in bikeTracking.WebWasm/Components/ that calls MSAL logout and clears authentication state
- [ ] T019 [US1] Create AuthorizeView.razor component in bikeTracking.WebWasm/Components/ to conditionally show/hide protected pages based on authentication status
- [ ] T020 [US1] Create LoginCallback.razor page in bikeTracking.WebWasm/Pages/Authentication/ to handle OAuth callback redirect (https://localhost:7265/authentication/login-callback)
- [ ] T021 [US1] Create LogoutCallback.razor page in bikeTracking.WebWasm/Pages/Authentication/ to handle post-logout redirect (https://localhost:7265/authentication/logout-callback)
- [ ] T022 [US1] Update App.razor root component to initialize MSAL on app startup and establish authentication state provider
- [ ] T023 [US1] Update Program.cs in bikeTracking.WebWasm to register MSAL.js configuration with Entra ID ClientID, Authority, and scopes
- [ ] T024 [US1] Update MainPage.razor to remain accessible to all users (authenticated and unauthenticated)
- [ ] T025 [US1] Update RidesPage.razor to use <AuthorizeView> wrapper; show only when user is authenticated
- [ ] T026 [US1] Update ProfilePage.razor to use <AuthorizeView> wrapper and display authenticated user's Entra ID profile information (oid, email, given_name, family_name)

#### US1 - Integration Tests

- [ ] T027 [US1] Create end-to-end login flow test in bikeTracking.Tests/Integration/LoginFlowTests.cs: unauthenticated user navigates → redirected to Entra ID → authenticates → returns to login-callback → authenticated state established
- [ ] T028 [US1] Create end-to-end logout flow test in bikeTracking.Tests/Integration/LogoutFlowTests.cs: authenticated user clicks logout → redirected to logout-callback → authentication state cleared → main page accessible
- [ ] T029 [US1] Create protected page access test in bikeTracking.Tests/Integration/ProtectedPageAccessTests.cs: verify unauthenticated users cannot access RidesPage/ProfilePage, authenticated users can

---

## Phase 3: User Story 2 - API Authentication & Authorization (P1)

### US2 Phase - Backend Token Validation

**Story Goal**: API endpoints validate Bearer tokens and reject unauthenticated requests; authenticated users can call protected endpoints

**Independent Test**: Call protected API endpoint with valid token (200) → with invalid token (401) → with no token (401) → with expired token (401)

**Test Strategy**: Unit tests for token validation + integration tests calling actual endpoints

#### US2 - Unit Tests

- [ ] T030 [US2] Create unit test for BearerTokenValidationHandler.ValidateToken() in bikeTracking.Tests/Authentication/BearerTokenValidationTests.cs
- [ ] T031 [US2] Create unit test for claims extraction from JWT token in bikeTracking.Tests/Authentication/ClaimsExtractionTests.cs
- [ ] T032 [US2] Create unit test for expired token rejection in bikeTracking.Tests/Authentication/TokenExpirationTests.cs
- [ ] T033 [US2] Create unit test for malformed token rejection in bikeTracking.Tests/Authentication/MalformedTokenTests.cs
- [ ] T034 [US2] Create unit test for missing Authorization header handling in bikeTracking.Tests/Authentication/MissingTokenTests.cs
- [ ] T034b [US2] Create unit test for 403 Forbidden response when user authenticated but not authorized in bikeTracking.Tests/Authentication/UnauthorizedUserTests.cs

#### US2 - API Middleware & Configuration

- [ ] T035 [P] [US2] Create BearerTokenAuthenticationHandler in bikeTracking.ApiService/Authentication/BearerTokenAuthenticationHandler.cs to validate JWT tokens from Entra ID
- [ ] T036 [US2] Create ClaimsPrincipalExtensions in bikeTracking.ApiService/Authentication/ClaimsPrincipalExtensions.cs with helper methods to extract user ID, email, display name from token claims
- [ ] T037 [US2] Create AuthenticationConfiguration class in bikeTracking.ApiService/Authentication/AuthenticationConfiguration.cs to encapsulate Entra ID settings (Authority, ClientID, Audience)
- [ ] T038 [US2] Update Program.cs in bikeTracking.ApiService to register Microsoft.Identity.Web authentication services with Entra ID configuration
- [ ] T039 [US2] Update Program.cs to add Bearer token validation middleware to request pipeline
- [ ] T040 [US2] Update RidesEndpoints.cs in bikeTracking.ApiService/Endpoints/ to decorate all protected endpoints with [Authorize] attribute

#### US2 - Protected Endpoints

- [ ] T041 [US2] Update GET /api/rides endpoint to require [Authorize] attribute and extract authenticated user ID from claims
- [ ] T042 [US2] Update POST /api/rides endpoint to require [Authorize] attribute and set ride owner to authenticated user ID
- [ ] T043 [US2] Update PUT /api/rides/{id} endpoint to require [Authorize] attribute and validate user owns the ride before updating
- [ ] T044 [US2] Update DELETE /api/rides/{id} endpoint to require [Authorize] attribute and validate user owns the ride before deleting
- [ ] T045 [US2] Create GET /api/me endpoint that returns authenticated user's profile information (ID, email, display name) from token claims
- [ ] T046 [US2] Keep GET /api/health or similar public endpoints accessible without [Authorize] for health checks

#### US2 - Integration Tests

- [ ] T047 [US2] Create integration test for protected API endpoint with valid Bearer token in bikeTracking.Tests/Integration/AuthenticationIntegrationTests.cs
- [ ] T048 [US2] Create integration test for protected API endpoint with invalid/malformed Bearer token (expect 401)
- [ ] T049 [US2] Create integration test for protected API endpoint with expired Bearer token (expect 401)
- [ ] T050 [US2] Create integration test for protected API endpoint with missing Authorization header (expect 401)
- [ ] T051 [US2] Create integration test for authenticated user's claims properly extracted and validated (user context available to endpoint logic)

---

## Phase 4: User Story 3 - Token Refresh & Session Management (P2)

### US3 Phase - Automatic Token Refresh

**Story Goal**: Tokens automatically refresh before expiration; users remain authenticated without re-login

**Independent Test**: Monitor token expiration → make API call before expiry → verify token silently refreshed → API succeeds

**Test Strategy**: Unit tests for refresh logic + integration tests for continuous API calls

#### US3 - Frontend Token Refresh

- [ ] T051 [US3] Update AuthenticationService in bikeTracking.WebWasm/Services/ to implement AcquireTokenSilentAsync() for automatic refresh via JavaScript interop
- [ ] T052 [US3] Update ApiClient in bikeTracking.WebWasm/Services/ to intercept API responses and refresh token on 401 if refresh token available
- [ ] T053 [US3] Create unit test for token refresh in bikeTracking.Tests/TokenRefreshTests.cs: verify tokens refresh before expiration
- [ ] T054 [US3] Create integration test for continuous API calls with expiring token in bikeTracking.Tests/Integration/TokenRefreshFlowTests.cs

#### US3 - Backend Token Validation Enhancement

- [ ] T055 [US3] Update BearerTokenAuthenticationHandler to log token refresh events to Application Insights for observability
- [ ] T056 [US3] Create endpoint POST /api/auth/refresh-token (if needed) to handle client-initiated refresh from frontend as fallback mechanism
- [ ] T057 [US3] Create unit test for token refresh validation in bikeTracking.Tests/Authentication/TokenRefreshValidationTests.cs

#### US3 - Integration & Observability

- [ ] T058 [US3] Create integration test for token expires and frontend transparently refreshes in bikeTracking.Tests/Integration/TransparentTokenRefreshTests.cs
- [ ] T059 [US3] Verify Application Insights logs all token refresh events (successful and failed) in bikeTracking.ApiService

---

## Phase 5: Edge Cases & Error Handling

### Edge Cases Phase

- [ ] T060 [P] Implement error handling when user authenticates but is not authorized for the application (user in Entra ID but not provisioned) — return 403 Forbidden
- [ ] T061 [P] Implement retry logic with exponential backoff for token refresh failures due to network issues
- [ ] T062 Implement UI notification when token refresh fails and user must re-authenticate (manual logout + redirect to login)
- [ ] T063 Implement user notification when Entra ID account is disabled during active session (attempt API call → 401 → graceful logout)
- [ ] T064 Log all edge case scenarios (unauthorized user, network failure, account disabled) to Application Insights

---

## Phase 6: Logging & Observability

### Observability Phase

- [ ] T065 [P] Add structured logging for authentication attempt (success/failure) to Application Insights in bikeTracking.ApiService/Authentication/BearerTokenAuthenticationHandler.cs
- [ ] T066 [P] Add logging for all 401 Unauthorized responses (missing, invalid, expired token) to Application Insights
- [ ] T067 [P] Add logging for all 403 Forbidden responses (user not provisioned for app) to Application Insights
- [ ] T068 Create custom Application Insights queries for: authentication success rate, token refresh rate, 401 error frequency, 403 authorization failures
- [ ] T069 Document monitoring strategy in specs/002-entra-auth-msal/monitoring.md with alert thresholds for auth failures

---

## Phase 7: Documentation & Deployment

### Documentation Phase

- [ ] T070 Create quickstart.md in specs/002-entra-auth-msal/ with:
  - Step-by-step local development setup (Entra ID app registration, config values)
  - How to run the Blazor application with MSAL authentication
  - Manual testing checklist for all user stories
  - Debugging tips for MSAL.js JavaScript interop and token validation issues

- [ ] T071 Create contracts/authentication-api.openapi.yml with OpenAPI 3.0 spec for all authentication endpoints:
  - POST /api/auth/logout (or handled by MSAL)
  - GET /api/me (authenticated user profile)
  - POST /api/auth/refresh-token (if implemented)

- [ ] T072 Create contracts/bearer-token-schema.json documenting expected JWT token format and claims (oid, email, given_name, family_name, exp, iat, etc.)

- [ ] T073 Create data-model.md in specs/002-entra-auth-msal/ documenting:
  - User principal entity structure (ID/oid, email, displayName, given_name, family_name)
  - Authentication audit log schema
  - Token management (MSAL managed in browser via JavaScript)

### Deployment Preparation

- [ ] T074 [P] Update CI/CD pipeline (.github/workflows/) to build and test both Blazor frontend and .NET backend with authentication enabled
- [ ] T075 [P] Configure production Entra ID app registration with production redirect URLs (if deploying to Azure)
- [ ] T076 Create deployment guide documenting how to configure Entra ID in production environment (Azure Key Vault for secrets, RBAC for app identity)

---

## Phase 8: Code Quality & Final Testing

### Code Quality Phase

- [ ] T078 [P] Run static analysis (SonarQube or similar) on authentication code to verify no secrets in code, proper error handling
- [ ] T077 [P] Run static analysis (SonarQube or similar) on authentication code to verify no secrets in code, proper error handling
- [ ] T078 [P] Run security scanning for OWASP top 10 vulnerabilities in authentication flow (XSS in token handling, CSRF in logout, etc.)
- [ ] T079 [P] Verify all authentication code follows constitutional Principle II (pure functions separated from auth boundary operations)
- [ ] T080 [P] Ensure all authentication code follows constitutional Principle VI (performance targets <500ms, observability via logging)

### Final Testing & Acceptance

- [ ] T081 [P] Run full end-to-end test scenario: unauthenticated user → login → access protected pages (RidesPage, ProfilePage) → logout → verify can only see main page
- [ ] T082 [P] Run security test: attempt to call protected API endpoints with invalid tokens, expired tokens, missing tokens (verify all rejected with 401); test 403 with unauthorized user
- [ ] T083 [P] Manual acceptance test with actual Entra ID user account: full login → access protected pages → token refresh → logout
- [ ] T084 Verify all requirements from spec.md acceptance scenarios are met and tested
- [ ] T085 Verify all success criteria from spec.md are met:
  - ✓ Users authenticate via Entra ID
  - ✓ All API endpoints reject unauthenticated requests with 401
  - ✓ All API endpoints reject unauthorized users with 403
  - ✓ Tokens are included in requests and validated
  - ✓ Token refresh works transparently
  - ✓ Protected pages hidden from unauthenticated users in Blazor UI
  - ✓ 100+ concurrent users supported
  - ✓ Auth decisions <500ms p95
  - ✓ 99% success rate without manual retry
  - ✓ All failures logged to Application Insights
---

## Task Execution Strategy

### Recommended Phase Order & Parallelization

**Phase 1 (Setup)**: Execute T001-T008 sequentially. Gate: All configuration in place before proceeding.

**Phase 2 (US1 - Frontend Auth)**: 
- Tests (T009-T013): Write tests first
- Implementation (T014-T026): Execute T014, T017, T019 in parallel; others can follow
- Integration Tests (T027-T029): After implementation
- **Deliverable**: Users can login via Entra ID and access protected pages

**Phase 3 (US2 - API Auth)**:
- Tests (T030-T034): Write tests first
- Implementation (T035-T046): Execute T035, T038 in parallel (both auth setup); then apply [Authorize] attributes
- Integration Tests (T047-T051): After implementation
- **Deliverable**: API endpoints validate Bearer tokens and reject unauthenticated requests

**Phase 4 (US3 - Token Refresh)**: Start after US1 & US2 complete
- Can execute in parallel with Phase 5 edge cases
- **Deliverable**: Token refresh works transparently without user re-login

**Phases 5-8**: Execute sequentially as final hardening, documentation, and acceptance

### MVP Scope (Phase 1 + 2 Core)

**Minimal Viable Product** = Phases 1-3, excluding:
- Advanced edge case handling (Phase 5)
- Full observability implementation (some of Phase 6)
- Comprehensive documentation (Phase 7)

MVP delivers: Users authenticate via Entra ID using Blazor UI, access protected pages, API validates Bearer tokens and rejects unauthorized users. Can be deployed and demonstrated to users.

### Success Metrics

- All tasks (T001-T085) completed and tested
- 0 authentication failures in manual acceptance testing
- API response time <500ms p95 with authentication overhead
- 100+ concurrent authenticated users sustainable
- 99%+ successful authentication flow completion rate
- All authentication events (login, logout, token refresh, 401, 403) logged to Application Insights
- Spec.md all acceptance scenarios verified
- Constitution checks re-pass post-implementation

---

**Next Steps**: Begin Phase 1 setup tasks. Execute in order: T001, then T002-T008 in parallel. Use Azure MCP tools for Entra ID verification and Application Insights configuration.
