# Feature Specification: Entra ID Authentication with MSAL

**Feature Branch**: `002-entra-auth-msal`  
**Created**: December 22, 2025  
**Status**: Draft  
**Input**: User description: "implement real authentication using my Entra ID app registration in the api and the web UI. use msal. ClientID: a9c55801-9ab4-45f9-a777-9320685f21ea, object id: 633fc7f8-8bea-4214-bdc6-76e4404b9cde, Directory id: 612b54fd-5aad-4fcc-add3-0b203ac0d9e0, redirect urls: https://localhost:7265/authentication"

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

1. **Given** a user is not authenticated and visits the web UI, **When** they access a protected page, **Then** they are redirected to the Microsoft Entra ID login page
2. **Given** a user enters valid Entra ID credentials on the login page, **When** they complete authentication, **Then** they are redirected back to the web UI and can access protected resources
3. **Given** a user is authenticated in the web UI, **When** they make API requests, **Then** the requests include a valid authentication token
4. **Given** a user's session expires, **When** they attempt to access protected resources, **Then** they are prompted to re-authenticate
5. **Given** a user clicks a logout button, **When** the action completes, **Then** their session is cleared and they cannot access protected resources without re-authenticating

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

- **FR-001**: System MUST redirect unauthenticated users to the Microsoft Entra ID login page when accessing protected resources in the web UI
- **FR-002**: System MUST accept and validate Entra ID authentication tokens in API requests via the Authorization header using Bearer token scheme
- **FR-003**: System MUST extract and validate claims from Entra ID tokens to determine user identity and permissions
- **FR-004**: System MUST provide a logout mechanism in the web UI that clears the user's session and authentication token
- **FR-005**: System MUST integrate MSAL (Microsoft Authentication Library) in the web UI for Entra ID authentication
- **FR-006**: System MUST integrate MSAL or equivalent token validation in the API backend for token verification
- **FR-007**: System MUST support token refresh when tokens approach expiration to maintain user sessions
- **FR-008**: System MUST include the authenticated user's identity information in API requests for audit and data isolation purposes
- **FR-009**: System MUST return 401 Unauthorized status for API requests with missing, invalid, or expired tokens
- **FR-010**: System MUST log all authentication attempts (successful and failed) for security auditing

### Key Entities

- **Authentication Token**: JWT or SAML token issued by Entra ID containing user identity and claims information
- **User Principal**: The authenticated user identity derived from Entra ID, including user ID, email, display name, and organizational claims
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

## Assumptions

1. **MSAL Library Selection**: MSAL.js is used for web UI authentication and MSAL.NET or similar for API validation
2. **User Authorization**: Initial implementation focuses on authentication (identity verification) only; authorization (role-based access) can be added in future phases
3. **Token Storage**: Web UI tokens are stored in secure, HttpOnly cookies or session storage as recommended by MSAL
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
