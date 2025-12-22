# Implementation Plan: Entra ID Authentication with MSAL

**Branch**: `002-entra-auth-msal` | **Date**: December 22, 2025 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/002-entra-auth-msal/spec.md`

## Summary

Implement real Entra ID authentication using MSAL across both the Vue.js frontend and .NET 10 API backend. Replace the current test authentication handler with a production-ready OAuth 2.0/OpenID Connect flow using the provided Entra ID app registration (ClientID: a9c55801-9ab4-45f9-a777-9320685f21ea). Unauthenticated users see only the main page; protected pages are hidden and redirect to login when accessed. The API validates all incoming tokens and rejects requests without valid Bearer tokens.

## Technical Context

**Language/Version**: Frontend: Vue.js (latest) with TypeScript; Backend: C# .NET 10  
**Primary Dependencies**: Frontend: MSAL.js (@azure/msal-browser, @azure/msal-vue); Backend: Microsoft.Identity.Web, Microsoft.AspNetCore.Authentication.OpenIdConnect  
**Storage**: N/A (authentication state stored in browser session/localStorage per MSAL best practices)  
**Testing**: Frontend: Vitest/Jest; Backend: xUnit with test authentication handlers  
**Target Platform**: Web (browser frontend, server-hosted API)  
**Project Type**: Web application (frontend + backend)  
**Performance Goals**: Authentication flow completes in <500ms; token validation adds <50ms to API requests  
**Constraints**: Bearer token must be included in Authorization header for all protected API calls; tokens expire within 1 hour (Entra ID default)  
**Scale/Scope**: Support 100+ concurrent authenticated users; apply to existing Rides API endpoints as first protected resource

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Principle II: Functional Programming
- ✓ PASS: Authentication is an impure boundary operation (identity verification at app edge)
- ✓ PASS: Business logic (Rides domain) remains pure and separate from auth concerns
- ✓ DECISION: Pure F# domain services will receive authenticated user context as parameter; no changes to domain layer structure required

### Principle IV: Quality-First Development
- ✓ PASS: Tests will be written first for all auth flows (login, logout, token validation, protected API calls)
- ✓ PASS: Integration tests verify end-to-end authentication flows before user acceptance
- ⚠️ REQUIRES CLARIFICATION: How should F# domain tests interact with .NET user identity claims?

### Principle V: User Experience & Accessibility
- ✓ PASS: Main page remains accessible to unauthenticated users
- ✓ PASS: Protected pages hidden from unauthenticated users (no 403 errors, just hidden from navigation)
- ✓ DECISION: Login/logout flows will use Fluent UI components for consistency

### Principle VI: Performance & Observability
- ✓ PASS: Bearer token validation must complete in <50ms (within <500ms API response budget)
- ✓ PASS: All authentication events (successful, failed, token refresh) will be logged to Application Insights
- ✓ DECISION: MSAL token caching enabled in browser; refresh tokens obtained from Entra ID

### Principle VII: Data Validation & Integrity
- ✓ PASS: Entra ID claims validation occurs server-side for all protected endpoints
- ✓ DECISION: User identity claims extracted from JWT token and validated before processing business logic

**GATE RESULT**: ✓ PASS — Feature aligns with all core principles. No violations or rejustifications needed.

## Project Structure

### Documentation (this feature)

```text
specs/002-entra-auth-msal/
├── plan.md                           # This file
├── spec.md                           # Feature specification
├── research.md                       # Phase 0: Technology decisions (to be created)
├── data-model.md                     # Phase 1: Authentication schema (to be created)
├── contracts/                        # Phase 1: API contracts (to be created)
│   ├── authentication-api.openapi.yml
│   └── user-context-dto.json
├── quickstart.md                     # Phase 1: Setup and testing guide (to be created)
├── checklists/
│   └── requirements.md               # Specification quality checklist
└── tasks.md                          # Phase 2: Implementation tasks (created by /speckit.tasks)
```

### Source Code Structure

```text
bikeTracking.ApiService/                                    # .NET 10 API backend
├── Authentication/
│   ├── AuthenticationConfiguration.cs                      # MSAL & JWT configuration
│   ├── ClaimsPrincipalExtensions.cs                       # User claim extraction helpers
│   └── BearerTokenAuthenticationHandler.cs                # Bearer token validation middleware
├── Endpoints/
│   ├── AuthenticationEndpoints.cs                         # Auth-related endpoints (login callback, token refresh)
│   ├── RidesEndpoints.cs                                  # Updated with [Authorize] attributes
│   └── HealthEndpoints.cs                                 # Public health check (no auth)
├── Middleware/
│   └── AuthenticationMiddleware.cs                        # Token extraction and validation (if not using built-in)
├── appsettings.json                                        # Entra ID config (ClientID, Authority, Audience)
├── appsettings.Development.json                           # Dev-specific overrides
├── Program.cs                                              # Updated with authentication service registration
└── tests/
    └── Authentication/
        ├── BearerTokenValidationTests.cs                  # Validate token handling
        └── AuthenticationIntegrationTests.cs              # End-to-end auth flow tests

bikeTracking.WebWasm/                                       # Vue.js frontend (Blazor WebAssembly)
├── src/
│   ├── components/
│   │   ├── LoginButton.vue                                # MSAL login trigger
│   │   ├── LogoutButton.vue                               # MSAL logout trigger
│   │   └── ProtectedPageGuard.vue                         # Route guard for authenticated pages
│   ├── pages/
│   │   ├── MainPage.vue                                   # Public main page
│   │   ├── RidesPage.vue                                  # Protected: list/manage rides
│   │   ├── ProfilePage.vue                                # Protected: user profile
│   │   └── LoginRedirect.vue                              # OAuth redirect handler
│   ├── router/
│   │   └── index.ts                                       # Route definitions with auth guards
│   ├── services/
│   │   ├── AuthenticationService.ts                       # MSAL.js wrapper for login/logout/token
│   │   ├── ApiClient.ts                                   # HTTP client with Bearer token injection
│   │   └── useAuth.ts                                     # Vue composable for authentication state
│   ├── stores/
│   │   └── authStore.ts                                   # Pinia store for auth state
│   ├── App.vue                                             # Root component with auth state check
│   └── main.ts                                             # App entry point with MSAL initialization
├── tests/
│   ├── unit/
│   │   ├── AuthenticationService.test.ts
│   │   └── useAuth.test.ts
│   └── integration/
│       └── LoginFlow.test.ts
└── public/
    └── index.html
```

**Structure Decision**: Web application with separate frontend (Vue.js in Blazor WebAssembly project) and backend (.NET 10 API). Authentication logic split: MSAL.js handles user login/logout in browser; Microsoft.Identity.Web validates Bearer tokens on API. Frontend routes guarded by auth composable; backend endpoints decorated with `[Authorize]` attribute.

## Phase 0: Research & Clarifications

*Status*: PENDING (create research.md with agent investigation)

### Research Tasks

1. **MSAL.js Configuration in Blazor WebAssembly Environment**
   - How does MSAL.js interact with Blazor WASM static assets?
   - Token storage strategy (session storage vs. IndexedDB vs. secure cookie)?
   - Redirect URI handling for `https://localhost:7265/authentication`

2. **Microsoft.Identity.Web Integration with .NET 10 Minimal APIs**
   - Bearer token validation middleware configuration
   - Claims extraction from JWT tokens
   - Token caching and refresh strategies

3. **Vue.js + Blazor WebAssembly Compatibility**
   - Can Vue.js run alongside Blazor components?
   - Build pipeline adjustments needed?
   - OR: Should authentication be implemented purely in Blazor components (no Vue)?

4. **Entra ID App Registration Validation**
   - Verify ClientID, Object ID, Directory ID are correct
   - Confirm redirect URL configured in app registration
   - Check application permissions and scopes needed (default = User.Read)

5. **Token Refresh & Session Management**
   - How to implement automatic token refresh without user intervention?
   - MSAL.js silent token acquisition vs. interactive refresh?
   - Handling token expiration in API requests (should frontend or backend retry?)

**GATE**: All research items must be resolved before proceeding to Phase 1 design.

## Phase 1: Design & Data Model

*Status*: PENDING (create data-model.md, contracts/, quickstart.md)

### Deliverables

1. **data-model.md**: Authentication schema
   - User principal entity (ID, email, displayName, objectId from Entra ID)
   - Authentication session tracking (if needed)
   - Token cache structure in browser (MSAL managed)
   - Audit log schema for authentication events (Success, Failure, TokenRefresh, Logout)

2. **contracts/**: API contracts
   - `authentication-api.openapi.yml`: Login/logout/token endpoints
   - `user-context-dto.json`: User identity shape returned from API (claims + profile data)
   - Bearer token format (JWT) and expected claims (oid, email, given_name, family_name)

3. **quickstart.md**: Setup & testing guide
   - Local development setup (Entra ID app registration configuration)
   - How to run the application with MSAL authentication
   - Manual testing checklist (login, logout, protected page access)
   - Integration test examples for CI/CD

### Constitution Check Re-evaluation

Post-design check will verify:
- Auth logic is isolated at application boundary (not mixed with domain logic)
- Pure F# domain services accept user context as parameter
- All authentication events are logged for observability
- Bearer token validation completes within performance budget
- Unauthenticated users cannot access protected endpoints (401 response, not 403 with hidden UI)

## Complexity Tracking

| Item | Rationale | Notes |
|------|-----------|-------|
| MSAL.js in Blazor WASM | Identity verification is an app boundary concern; MSAL is industry-standard for Entra ID | Alternative: Manual OAuth 2.0 flow (more complex, less maintainable) |
| Bearer token validation middleware | API endpoints must reject unauthenticated requests consistently | Alternative: Manual header parsing per endpoint (error-prone, repetitive) |
| Vue.js + Blazor compatibility | User specified Vue.js as frontend framework | If Vue.js incompatible with Blazor WASM, consider pure Blazor components with Fluent UI |
| Audit logging for auth events | Constitution Principle VI requires observability; security audits require auth logs | Alternative: No logging (fails compliance and security requirements) |

---

**Next Steps**: Proceed to Phase 0 research to clarify technology integration points. Then Phase 1 design will establish API contracts and authentication schema. Phase 2 (via /speckit.tasks) will generate implementation tasks.

