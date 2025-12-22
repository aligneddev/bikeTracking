# Implementation Plan: Entra ID Authentication with MSAL

**Branch**: `002-entra-auth-msal` | **Date**: December 22, 2025 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/002-entra-auth-msal/spec.md`

## Summary

Implement real Entra ID authentication using MSAL across both the Blazor frontend and .NET 10 API backend. Replace the current test authentication handler with a production-ready OAuth 2.0/OpenID Connect flow using the provided Entra ID app registration (ClientID: a9c55801-9ab4-45f9-a777-9320685f21ea). Unauthenticated users see only the main page; protected pages are hidden and redirect to login when accessed. The API validates all incoming tokens and rejects requests without valid Bearer tokens. Users authenticated in Entra ID but not authorized for this application receive 403 Forbidden responses.

## Technical Context

**Language/Version**: Frontend: C# Blazor WebAssembly with Razor components; Backend: C# .NET 10  
**Primary Dependencies**: Frontend: MSAL.js (@azure/msal-browser), Fluent UI Blazor components; Backend: Microsoft.Identity.Web, Microsoft.AspNetCore.Authentication.OpenIdConnect  
**Storage**: N/A (authentication state stored in browser session/localStorage per MSAL best practices)  
**Testing**: Frontend: xUnit with Blazor testing library; Backend: xUnit with test authentication handlers  
**Target Platform**: Web (browser frontend, server-hosted API)  
**Project Type**: Web application (frontend + backend)  
**Performance Goals**: Authentication flow completes in <500ms; token validation adds <50ms to API requests (architectural targets, not formal test gates)  
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
- ✓ PASS: Protected pages hidden from unauthenticated users (no 403 errors to UI, just hidden from navigation)
- ✓ PASS: Users authenticated but unauthorized receive clear 403 Forbidden response from API
- ✓ DECISION: Login/logout flows will use Fluent UI Blazor components for consistency

### Principle VI: Performance & Observability
- ✓ PASS: Bearer token validation must complete in <50ms (within <500ms API response budget)
- ✓ PASS: All authentication events (successful, failed, token refresh) will be logged to Application Insights
- ✓ DECISION: MSAL token caching enabled in browser; refresh tokens obtained from Entra ID

### Principle VII: Data Validation & Integrity
- ✓ PASS: Entra ID claims validation occurs server-side for all protected endpoints
- ✓ DECISION: User identity claims extracted from JWT token and validated before processing business logic

**GATE RESULT**: ✓ PASS — Feature aligns with all core principles. No violations or rejustifications needed.
Helpful Azure Resources

**Use Azure MCP tools for these tasks**:
- Entra ID app registration verification and configuration: Use Azure portal or `az` CLI commands (Azure MCP can help generate CLI commands)
- Token validation troubleshooting: AppLens diagnostic tool can help debug token-related issues
- Application Insights setup for logging: Azure MCP for resource provisioning
- Bearer token claims validation best practices: Consult Azure best practices MCP

## 
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

bikeTracking.WebWasm/                                       # Blazor WebAssembly frontend
├── Pages/
│   ├── Authentication/
│   │   ├── LoginCallback.razor                            # Callback page after Entra ID login (https://localhost:7265/authentication/login-callback)
│   │   └── LogoutCallback.razor                           # Callback page after logout (https://localhost:7265/authentication/logout-callback)
│   ├── MainPage.razor                                     # Public main page (accessible to all)
│   ├── RidesPage.razor                                    # Protected page (authenticated only)
│   └── ProfilePage.razor                                  # Protected page (authenticated only)
├── Components/
│   ├── LoginButton.razor                                  # MSAL login trigger component
│   ├── LogoutButton.razor                                 # MSAL logout trigger component
│   ├── AuthorizeView.razor                                # Components for auth-based rendering
│   └── NotAuthorized.razor                                # Fallback for unauthorized users
├── Services/
│   ├── AuthenticationService.cs                           # MSAL.js wrapper for login/logout/token acquisition via JavaScript interop
│   └── ApiClient.cs                                       # HTTP client with Bearer token injection
├── App.razor                                               # Root component with authentication state provider
├── Program.cs                                              # App initialization with MSAL configuration
└── tests/
    ├── Authentication/
    │   ├── AuthenticationServiceTests.cs
    │   └── UnauthorizedUserTests.cs
    ├── Components/
    │   ├── LoginButtonTests.cs
    │   └── AuthorizeViewTests.cs
    └── Integration/
        ├── LoginFlowTests.cs
        ├── LogoutFlowTests.cs
        └── ProtectedPageAccessTests.cs
```

**Structure Decision**: Blazor WebAssembly application with MSAL.js integration for authentication. Authentication logic split: MSAL.js handles user login/logout in browser via JavaScript interop; Microsoft.Identity.Web validates Bearer tokens on API. Frontend uses AuthorizeView components to guard protected pages; backend endpoints decorated with `[Authorize]` attribute.

## Phase 0: Research & Clarifications

*Status*: COMPLETED (user clarifications incorporated Dec 22, 2025)

### Resolved Clarifications

1. ✓ **Frontend Framework**: Pure Blazor WebAssembly with C# and Razor components (not Vue.js)
2. ✓ **OAuth Callback URLs**: 
   - Login: `https://localhost:7265/authentication/login-callback`
   - Logout: `https://localhost:7265/authentication/logout-callback`
3. ✓ **Unauthorized User Behavior**: Return 403 Forbidden (not authentication error) when user exists in Entra ID but not provisioned for app
4. ✓ **Performance Testing**: Not required; <500ms and <50ms are architectural targets, not formal test gates

### Remaining Research Items

1. **MSAL.js Configuration in Blazor WebAssembly Environment**
   - How does MSAL.js interact with Blazor WASM via JavaScript interop?
   - Token storage strategy (IndexedDB, session storage)?
   - Callback URI handling for dual routes (login/logout)

2. **Microsoft.Identity.Web Integration with .NET 10 Minimal APIs**
   - Bearer token validation middleware configuration
   - Claims extraction from JWT tokens
   - 403 Forbidden authorization policy implementation

3. **Entra ID App Registration Validation**
   - Verify ClientID, Object ID, Directory ID accuracy
   - Confirm both redirect URLs configured in app registration
   - Validate application permissions

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

