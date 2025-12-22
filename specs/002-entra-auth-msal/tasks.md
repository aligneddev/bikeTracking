# Implementation Tasks: Entra ID Authentication with MSAL

**Feature**: `002-entra-auth-msal` | **Spec**: [spec.md](spec.md) | **Plan**: [plan.md](plan.md)

**Feature Description**: Implement real Entra ID authentication using MSAL in both Vue.js frontend and .NET 10 API backend. Replace test authentication handler with production-ready OAuth 2.0/OpenID Connect flow.

**Entra ID Configuration**:
- ClientID: `a9c55801-9ab4-45f9-a777-9320685f21ea`
- Object ID: `633fc7f8-8bea-4214-bdc6-76e4404b9cde`
- Directory ID: `612b54fd-5aad-4fcc-add3-0b203ac0d9e0`
- Redirect URL: `https://localhost:7265/authentication`

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

- [ ] T009 [US1] Create unit test for AuthenticationService.login() in bikeTracking.WebWasm/tests/unit/AuthenticationService.test.ts
- [ ] T010 [US1] Create unit test for AuthenticationService.logout() in bikeTracking.WebWasm/tests/unit/AuthenticationService.test.ts
- [ ] T011 [US1] Create unit test for AuthenticationService.getAccessToken() in bikeTracking.WebWasm/tests/unit/AuthenticationService.test.ts
- [ ] T012 [US1] Create unit test for useAuth() composable authentication state in bikeTracking.WebWasm/tests/unit/useAuth.test.ts
- [ ] T013 [US1] Create unit test for route guard isAuthenticated check in bikeTracking.WebWasm/tests/unit/authGuard.test.ts

#### US1 - Frontend Implementation

- [ ] T014 [P] [US1] Create AuthenticationService wrapper class in bikeTracking.WebWasm/src/services/AuthenticationService.ts that wraps MSAL.js for login/logout/token acquisition
- [ ] T015 [US1] Create Pinia authentication store in bikeTracking.WebWasm/src/stores/authStore.ts to track user identity, authentication status, and access token
- [ ] T016 [US1] Create useAuth() Vue composable in bikeTracking.WebWasm/src/services/useAuth.ts to provide reactive authentication state in components
- [ ] T017 [P] [US1] Create LoginButton component in bikeTracking.WebWasm/src/components/LoginButton.vue that triggers MSAL authentication flow
- [ ] T018 [P] [US1] Create LogoutButton component in bikeTracking.WebWasm/src/components/LogoutButton.vue that calls MSAL logout and clears local state
- [ ] T019 [US1] Create ProtectedPageGuard component in bikeTracking.WebWasm/src/components/ProtectedPageGuard.vue that hides protected pages from unauthenticated users
- [ ] T020 [US1] Create LoginRedirect page in bikeTracking.WebWasm/src/pages/LoginRedirect.vue to handle OAuth callback redirect (https://localhost:7265/authentication)
- [ ] T021 [US1] Update App.vue root component to initialize MSAL on app startup and check authentication state
- [ ] T022 [US1] Update main.ts in bikeTracking.WebWasm/src/ to configure MSAL.js with Entra ID ClientID, Authority, and scopes
- [ ] T023 [US1] Update router/index.ts in bikeTracking.WebWasm to add auth guards to protected routes (RidesPage, ProfilePage) and mark main page as public
- [ ] T024 [US1] Update RidesPage.vue to show only when user is authenticated; hide page from unauthenticated users via route guard
- [ ] T025 [US1] Update ProfilePage.vue to show only when user is authenticated and display authenticated user's Entra ID profile information
- [ ] T026 [US1] Update MainPage.vue to remain accessible to all users (authenticated and unauthenticated)

#### US1 - Integration Tests

- [ ] T027 [US1] Create end-to-end login flow test in bikeTracking.WebWasm/tests/integration/LoginFlow.test.ts: unauthenticated user → login redirect → authenticate → return to app
- [ ] T028 [US1] Create end-to-end logout flow test in bikeTracking.WebWasm/tests/integration/LogoutFlow.test.ts: authenticated user → logout → session cleared → redirect to main page
- [ ] T029 [US1] Create protected page access test in bikeTracking.WebWasm/tests/integration/ProtectedPageAccess.test.ts: verify unauthenticated users see only main page, authenticated users see protected pages

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

- [ ] T052 [US3] Update AuthenticationService in bikeTracking.WebWasm/src/services/AuthenticationService.ts to implement acquireTokenSilent() for automatic refresh
- [ ] T053 [US3] Update ApiClient in bikeTracking.WebWasm/src/services/ApiClient.ts to intercept API responses and refresh token on 401 if refresh token available
- [ ] T054 [US3] Create unit test for token refresh in bikeTracking.WebWasm/tests/unit/TokenRefreshService.test.ts: verify tokens refresh before expiration
- [ ] T055 [US3] Create integration test for continuous API calls with expiring token in bikeTracking.WebWasm/tests/integration/TokenRefreshFlow.test.ts

#### US3 - Backend Token Validation Enhancement

- [ ] T056 [US3] Update BearerTokenAuthenticationHandler to log token refresh events to Application Insights for observability
- [ ] T057 [US3] Create endpoint POST /api/auth/refresh-token (if needed) to handle client-initiated refresh from frontend as fallback mechanism
- [ ] T058 [US3] Create unit test for token refresh validation in bikeTracking.Tests/Authentication/TokenRefreshTests.cs

#### US3 - Integration & Observability

- [ ] T059 [US3] Create integration test for token expires and frontend transparently refreshes in bikeTracking.WebWasm/tests/integration/TransparentTokenRefresh.test.ts
- [ ] T060 [US3] Verify Application Insights logs all token refresh events (successful and failed) in bikeTracking.ApiService

---

## Phase 5: Edge Cases & Error Handling

### Edge Cases Phase

- [ ] T061 [P] Implement error handling when user authenticates but is not authorized for the application (user in Entra ID but not provisioned)
- [ ] T062 [P] Implement retry logic with exponential backoff for token refresh failures due to network issues
- [ ] T063 Implement UI notification when token refresh fails and user must re-authenticate (manual logout + redirect to login)
- [ ] T064 Implement user notification when Entra ID account is disabled during active session (attempt API call → 401 → graceful logout)
- [ ] T065 Log all edge case scenarios (unauthorized user, network failure, account disabled) to Application Insights

---

## Phase 6: Logging & Observability

### Observability Phase

- [ ] T066 [P] Add structured logging for authentication attempt (success/failure) to Application Insights in bikeTracking.ApiService/Authentication/BearerTokenAuthenticationHandler.cs
- [ ] T067 [P] Add metrics for authentication flow completion time (<500ms target) to Application Insights
- [ ] T068 [P] Add metrics for token validation latency (<50ms target) per API request to Application Insights
- [ ] T069 Create custom Application Insights queries for: authentication success rate, token refresh rate, 401 error frequency, average auth latency
- [ ] T070 Document monitoring strategy in specs/002-entra-auth-msal/monitoring.md with alert thresholds for auth failures

---

## Phase 7: Documentation & Deployment

### Documentation Phase

- [ ] T071 Create quickstart.md in specs/002-entra-auth-msal/ with:
  - Step-by-step local development setup (Entra ID app registration, config values)
  - How to run the application with MSAL authentication
  - Manual testing checklist for all user stories
  - Debugging tips for MSAL.js and token validation issues

- [ ] T072 Create contracts/authentication-api.openapi.yml with OpenAPI 3.0 spec for all authentication endpoints:
  - POST /api/auth/login (or handled by MSAL redirect)
  - POST /api/auth/logout (or handled by MSAL)
  - GET /api/me (user profile)
  - POST /api/auth/refresh-token (if implemented)

- [ ] T073 Create contracts/bearer-token-schema.json documenting expected JWT token format and claims (oid, email, given_name, family_name, exp, iat, etc.)

- [ ] T074 Create data-model.md in specs/002-entra-auth-msal/ documenting:
  - User principal entity structure (ID, email, displayName, objectId)
  - Authentication audit log schema
  - Token cache structure (MSAL managed in browser)

### Deployment Preparation

- [ ] T075 [P] Update CI/CD pipeline (.github/workflows/) to build and test both frontend and backend with authentication enabled
- [ ] T076 [P] Configure production Entra ID app registration with production redirect URLs (if deploying to Azure)
- [ ] T077 Create deployment guide documenting how to configure Entra ID in production environment (Azure Key Vault for secrets, RBAC for app identity)

---

## Phase 8: Code Quality & Final Testing

### Code Quality Phase

- [ ] T078 [P] Run static analysis (SonarQube or similar) on authentication code to verify no secrets in code, proper error handling
- [ ] T079 [P] Run security scanning for OWASP top 10 vulnerabilities in authentication flow (XSS in token handling, CSRF in logout, etc.)
- [ ] T080 [P] Verify all authentication code follows constitutional Principle II (pure functions separated from auth boundary operations)
- [ ] T081 [P] Ensure all authentication code follows constitutional Principle VI (performance <500ms, observability via logging)

### Final Testing & Acceptance

- [ ] T082 [P] Run full end-to-end test scenario: unauthenticated user → login → access rides → logout → verify can only see main page
- [ ] T083 [P] Run performance test with 100 concurrent authenticated users accessing API endpoints (verify <500ms response time)
- [ ] T084 [P] Run security test: attempt to call protected endpoints with invalid tokens, expired tokens, missing tokens (verify all rejected with 401)
- [ ] T085 [P] Manual acceptance test with actual Entra ID user account: full login → access protected pages → token refresh → logout
- [ ] T086 Verify all requirements from spec.md acceptance scenarios are met and tested
- [ ] T087 Verify all success criteria from spec.md are met:
  - ✓ Users authenticate via Entra ID
  - ✓ All API endpoints reject unauthenticated requests
  - ✓ Tokens are included in requests and validated
  - ✓ Token refresh works transparently
  - ✓ 100+ concurrent users supported
  - ✓ Auth decisions <500ms
  - ✓ 99% success rate without manual retry
  - ✓ All failures logged

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

MVP delivers: Users authenticate via Entra ID, access protected pages, API validates tokens. Can be deployed and demonstrated to users.

### Success Metrics

- All 87 tasks completed and tested
- 0 authentication failures in manual acceptance testing
- API response time <500ms p95 with authentication overhead
- 100+ concurrent authenticated users sustainable
- 99%+ successful authentication flow completion rate
- All authentication events logged to Application Insights
- Spec.md all acceptance scenarios verified
- Constitution checks re-pass post-implementation

---

**Next Steps**: Begin Phase 1 setup tasks. Execute in order: T001, then T002-T008 in parallel.
