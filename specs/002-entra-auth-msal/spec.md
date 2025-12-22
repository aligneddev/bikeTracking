# Feature Specification: Entra ID Authentication with MSAL

**Feature Branch**: `002-entra-auth-msal`  
**Created**: December 22, 2025  
**Status**: Draft  
**Input**: User description: "implement real authentication using my Entra ID app registration in the api and the web UI. use msal. ClientID: a9c55801-9ab4-45f9-a777-9320685f21ea, object id: 633fc7f8-8bea-4214-bdc6-76e4404b9cde, Directory id: 612b54fd-5aad-4fcc-add3-0b203ac0d9e0"

**Clarifications (Dec 22, 2025)**:
- Frontend: Pure Blazor (C# + Razor components, no Vue.js)
- Callback URLs: `https://localhost:7265/authentication/login-callback` and `https://localhost:7265/authentication/logout-callback`
- Unauthorized behavior: Return 403 Forbidden when user exists in Entra ID but not authorized for app
- No performance testing required

## User Scenarios & Testing *(mandatory)*

<!--
  IMPORTANT: User stories should be PRIORITIZED as user journeys ordered by importance.
  Each user story/journey must be INDEPENDENTLY TESTABLE - meaning if you implement just ONE of them,
  you should still have a viable MVP (Minimum Viable Product) that delivers value.
  
  Assign priorities (P1, P2, P3, etc.) to each story, where P1 is the most critical.
  Think of each story as a standalone slice of functionality that can be:
  - Developed independently
  - Tested independently
  - Deployed independently
  - Demonstrated to users independently
-->

### User Story 1 - Web UI User Authentication (Priority: P1)

A user visiting the web UI needs to authenticate using their Microsoft Entra ID credentials rather than using the current test authentication handler.

**Why this priority**: Web UI authentication is the primary user-facing feature that enables users to access the application securely with their organization identity.

**Independent Test**: Can be fully tested by opening the web UI in a browser, being redirected to the Microsoft login page, authenticating with valid Entra ID credentials, and being redirected back to the application with an authenticated session.

**Acceptance Scenarios**:

1. **Given** a user is not authenticated, **When** they visit the web UI, **Then** they can only view the main page and all other pages are hidden/inaccessible
2. **Given** a user is not authenticated and tries to navigate to a protected page, **When** they attempt access, **Then** they are redirected to the Microsoft Entra ID login page
3. **Given** a user enters valid Entra ID credentials on the login page, **When** they complete authentication, **Then** they are redirected back to the web UI and can access all protected resources
4. **Given** a user is authenticated in the web UI, **When** they make API requests, **Then** the requests include a valid authentication token via the Authorization header
5. **Given** a user's session expires, **When** they attempt to access protected resources, **Then** they are prompted to re-authenticate
6. **Given** a user clicks a logout button, **When** the action completes, **Then** their session is cleared, they return to the main page, and all protected pages are hidden again without re-authenticating
7. **Given** a user is authenticated in Entra ID but not provisioned for this application, **When** they attempt to access the API, **Then** the API returns a 403 Forbidden response

---

### User Story 2 - API Authentication & Authorization (Priority: P1)

API endpoints need to validate incoming authentication tokens and ensure only authenticated users can access protected resources.

**Why this priority**: API authentication is fundamental to securing backend resources and preventing unauthorized access.

**Independent Test**: Can be fully tested by making HTTP requests to protected API endpoints with valid tokens (they succeed), invalid tokens (they fail), and no tokens (they fail).

**Acceptance Scenarios**:

1. **Given** a request to a protected API endpoint without a token, **When** the request is received, **Then** the API returns a 401 Unauthorized response
2. **Given** a request to a protected API endpoint with a valid authentication token, **When** the request is received, **Then** the API processes the request and returns a 200 OK response with the requested data
3. **Given** a request to a protected API endpoint with an expired token, **When** the request is received, **Then** the API returns a 401 Unauthorized response
4. **Given** a request to a protected API endpoint with a malformed or invalid token, **When** the request is received, **Then** the API returns a 401 Unauthorized response

---

### User Story 3 - Token Refresh & Session Management (Priority: P2)

The application should automatically handle token refresh when tokens are near expiration, maintaining user sessions without requiring manual re-authentication.

**Why this priority**: This improves user experience by preventing unexpected logouts during normal usage while maintaining security.

**Independent Test**: Can be fully tested by monitoring token expiration times and verifying that tokens are refreshed transparently before expiration.

**Acceptance Scenarios**:

1. **Given** a user has a valid but expiring token, **When** they make an API request before expiration, **Then** the system automatically refreshes the token without interrupting the user's workflow
2. **Given** a user's token has fully expired and cannot be refreshed, **When** they attempt to make an API request, **Then** they are prompted to re-authenticate

---

### Edge Cases

- What happens when a user authenticates but is not authorized for the application (user exists in Entra ID but not provisioned for this app)?
- How does the system handle network failures during token refresh?
- What is the behavior if a user's Entra ID account is disabled after they have authenticated?
- How does the application handle users with multiple Entra ID tenants?

## Requirements *(mandatory)*

<!--
  ACTION REQUIRED: The content in this section represents placeholders.
  Fill them out with the right functional requirements.
-->

### Functional Requirements

- **FR-001**: System MUST hide all protected pages and resources from unauthenticated users, showing only the main page as accessible
- **FR-002**: System MUST redirect unauthenticated users to the Microsoft Entra ID login page when they attempt to navigate to protected pages or resources
- **FR-003**: System MUST accept and validate Emandatory claims from Entra ID tokens (oid, email) to determine user identity; optional claims include given_name, family_name, organization
- **FR-005**: System MUST provide a logout mechanism in the web UI that clears the user's session and authentication token, returning unauthenticated users to the main page
- **FR-006**: System MUST integrate MSAL (Microsoft Authentication Library) in the Blazor web UI for Entra ID authentication
- **FR-007**: System MUST integrate Microsoft.Identity.Web in the .NET 10 API backend for Bearer token validation
- **FR-008**: System MUST support token refresh when tokens approach expiration to maintain user sessions without interrupting user workflow
- **FR-009**: System MUST include the authenticated user's identity information (extracted from JWT claims) in API requests for audit and data isolation purposes
- **FR-010**: System MUST return 401 Unauthorized status for API requests with missing, invalid, or expired tokens
- **FR-011**: System MUST return 403 Forbidden status for API requests from authenticated users (valid token) who are not authorized to access the application
- **FR-012**: System MUST return 401 Unauthorized status for API requests with missing, invalid, or expired tokens
- **FR-011**: System MUST log all authentication attempts (successful and failed) for security auditing

### Key Entities
token issued by Entra ID containing user identity claims
  - **Mandatory Claims**: `oid` (user object ID), `email` (user email address)
  - **Optional Claims**: `given_name`, `family_name`, `organization`
  - **Expiration**: tokens expire per Entra ID configuration (default ~1 hour)
- **User Principal**: The authenticated user identity derived from Entra ID claims (oid, email, given_name, family_name)
- **Authentication Context**: Runtime context containing the current user IDID, including user ID, email, display name, and organizational claims
- **Authentication Context**: Runtime context containing the current user, token expiration time, and refresh token information

## Success Criteria *(mandatory)*

- Users can successfully authenticate using their Entra ID credentials and access the web UI
- All API endpoints requiring authentication reject requests without valid tokens
- Authenticated users can make API calls with their token included in the Authorization header
- Token expiration and refresh mechanisms work transparently without user intervention
- Application supports at least 100 concurrent authenticated users
- Authentication and authorization decisions complete within 500ms
- 99% of successful authentication flows complete without requiring manual retry
- All authentication failures are logged with sufficient detail for security investigations
Frontend Framework**: Blazor WebAssembly with C# and Razor components (not Vue.js)
2. **MSAL Library Selection**: MSAL.js is used in Blazor frontend for authentication; Microsoft.Identity.Web is used in .NET 10 API backend
3. **OAuth Callback URLs**: Two explicit callback routes configured:
   - `https://localhost:7265/authentication/login-callback` — handles successful Entra ID login redirect
   - `https://localhost:7265/authentication/logout-callback` — handles post-logout redirect
4. **User Authorization**: Initial implementation focuses on authentication (identity verification) only; role-based access control can be added in future phases
5. **Token Storage**: MSAL.js handles token storage securely in browser session/local storage per library best practices
6. **Unauthorized User Behavior**: Users authenticated in Entra ID but not provisioned for this app receive 403 Forbidden responses on API calls
7. **Entra ID App Registration**: The provided app registration (ClientID: a9c55801-9ab4-45f9-a777-9320685f21ea, Object ID: 633fc7f8-8bea-4214-bdc6-76e4404b9cde, Directory ID: 612b54fd-5aad-4fcc-add3-0b203ac0d9e0) is pre-configured with appropriate permissions and reply URLs
8. **Tenant Configuration**: The application operates within a single Entra ID tenant context
9. **Performance**: No explicit performance testing required; <500ms and <50ms targets are architectural goals, not formal test gates as recommended by MSAL
4. **Redirect URL**: The configured redirect URL https://localhost:7265/authentication is accessible during development; production URLs will be configured separately
5. **Entra ID App Registration**: The provided app registration (ClientID, Object ID, Directory ID) is already configured with appropriate permissions and reply URLs
6. **Tenant Configuration**: The application operates within a single Entra ID tenant context

## Out of Scope

- Role-based access control (RBAC) or fine-grained authorization policies
- Support for guest or external user identities (B2B scenarios)
- Multi-tenant support requiring dynamic tenant switching
- Custom claims or advanced token customization
- SAML 2.0 support (OIDC/OAuth2 only)
- Legacy authentication methods or non-standard protocols
