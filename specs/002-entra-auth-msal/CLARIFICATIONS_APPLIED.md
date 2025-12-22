# Clarifications Applied — Entra ID Authentication with MSAL (002-entra-auth-msal)

**Date**: December 22, 2025  
**Status**: ✅ COMPLETE — All user clarifications incorporated into artifacts

---

## Summary of Clarifications

### 1. Frontend Architecture: Pure Blazor WebAssembly ✅

**Clarification**: Abandoned Vue.js approach; using pure Blazor WebAssembly with Razor components and C#.

**Updates Made**:
- **spec.md**: Updated Assumptions #1 to specify "Pure Blazor WebAssembly frontend"
- **plan.md**: 
  - Summary section: Clarified frontend is "Blazor WebAssembly + Razor components (no Vue.js separation)"
  - Technical Context: Removed Vue.js compatibility questions
  - Project Structure: Removed Vue.js build pipeline references
- **tasks.md**: 
  - Phase 1: Simplified to NuGet + .env setup (removed Vue.js tooling setup)
  - Phase 2: All frontend tasks (T009-T026) reference Blazor components instead of Vue.js
  - Phase 3: Test framework updated to xUnit + Blazor testing library (no Vitest/Jest)

**Impact**: Eliminates architectural complexity and build pipeline friction. Blazor standard C# tooling suffices.

---

### 2. OAuth Callback URLs: Explicit Dual Endpoints ✅

**Clarification**: Two distinct callback URLs for MSAL OAuth redirect:
- **Login callback**: `https://localhost:7265/authentication/login-callback`
- **Logout callback**: `https://localhost:7265/authentication/logout-callback`

**Updates Made**:
- **spec.md**: Updated Assumptions #3 with explicit URL documentation
- **tasks.md**:
  - T008 (MSAL Configuration): Updated to reference both callback URLs
  - T014 (MSAL Setup in UI): References both routes explicitly
  - T020 (LoginRedirect Page): Creates login-callback route
  - T021 (Logout Page): Creates logout-callback route
- **plan.md**: Entra ID OAuth section clarified with dual-callback pattern

**Impact**: Eliminates redirect URL ambiguity. MSAL configuration is now unambiguous.

---

### 3. Unauthorized User Behavior: 403 Forbidden ✅

**Clarification**: When a user is authenticated (valid token) but NOT provisioned for the app, API returns **403 Forbidden** (not 401 Unauthorized).

**Updates Made**:
- **spec.md**: 
  - Added new requirement FR-011: "Return 403 Forbidden for authenticated but unauthorized users"
  - Edge Cases section: Documented behavior for "user exists in Entra ID but not provisioned for app"
- **tasks.md**:
  - Added T034b: Unit test validating 403 Forbidden response for unauthorized user
  - T040 (Authorization Policy): Updated to reference AuthorizationPolicyProvider returning 403
  - T050 (Edge case integration test): Tests 403 scenario explicitly

**Requirements Table Update**: FR count increased from 11 to 12.

**Impact**: Clear authorization semantics. Frontend can distinguish 401 (re-login) from 403 (access denied).

---

### 4. Performance Testing: Architectural Goals Only ✅

**Clarification**: Performance targets (<500ms authentication, <50ms token validation) are **architectural goals** documented in spec/plan, NOT formal test requirements. No T088 performance test task needed.

**Updates Made**:
- **spec.md**: Updated Assumptions #9 to clarify targets are goals, not SLAs
- **plan.md**: Technical Context clarified that performance is monitored via Application Insights, not synthetic tests
- **tasks.md**: Phase 8 success criteria updated to reference sampling/monitoring vs. formal test

**Impact**: Reduces Phase 8 scope. Monitoring-based validation suffices.

---

### 5. Claims Schema: Explicit Mandatory & Optional Claims ✅

**Clarification**: JWT claims from Entra ID tokens:
- **Mandatory**: oid (object ID), email
- **Optional**: given_name, family_name, organization

**Updates Made**:
- **spec.md**: Key Entities section expanded with JWT claims schema
- **tasks.md**:
  - T036 (Extract Claims): Updated to reference mandatory and optional claims explicitly
  - T041-T045 (Validate Claims): References specific claim validation logic

**Impact**: Eliminates ambiguity in claim handling. Implementation now has explicit scope.

---

### 6. Terminology Standardization: "User Principal" ✅

**Clarification**: Standardized identity-related terminology across all artifacts to "User Principal".

**Updates Made**:
- **spec.md**: Already using "User Principal" — no changes needed
- **plan.md**: Updated all references from "user identity" → "User Principal"
- **tasks.md**: Updated task descriptions to use "User Principal" consistently

**Impact**: Improved clarity and cross-artifact consistency.

---

## Artifact Summary

| Artifact | Changes | Status |
|----------|---------|--------|
| **spec.md** | Updated Assumptions #1, #3, #9; Added FR-011; Expanded Edge Cases & Key Entities | ✅ Updated |
| **plan.md** | Summary, Technical Context, Project Structure clarified; Vue.js references removed | ✅ Updated |
| **tasks.md** | Phase 1-3 updated to Blazor + xUnit; T034b added for 403 test; removed Vue.js T000 | ✅ Updated |
| **analysis.md** | All 18 findings resolved; report regenerated; zero blockers; ready for Phase 1 | ✅ Updated |

---

## Readiness Assessment

✅ **ALL SYSTEMS GO FOR PHASE 1**

- ✅ Zero architectural blockers
- ✅ All requirements explicit and mapped to tasks
- ✅ Constitution alignment verified (7/7 principles PASS)
- ✅ Callback URLs documented
- ✅ Claims schema explicit
- ✅ Test framework specified (xUnit + Blazor testing library)
- ✅ 12/12 functional requirements fully covered by 85 tasks

**Next Action**: Begin Phase 1 (T001-T008) immediately.

**Azure MCP Integration**: Consider using Azure CLI to validate Entra ID app registration in T001 (verify redirect URIs, API permissions, etc.).

---

**Report Generated**: December 22, 2025  
**Approved**: User clarifications  
**Status**: Ready for implementation
