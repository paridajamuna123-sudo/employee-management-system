---
trigger: always_on
description: Security, authentication, authorization (RBAC), and employee PII/salary protection standards.
---

# Security & Compliance Standards

## 1. Authentication & JWT Management
- **Identity Foundation**: Use ASP.NET Core Identity for user management, role membership, and password hashing (PBKDF2 with SHA-256/SHA-512).
- **JWT Best Practices**:
  - Sign JWT tokens with HMAC-SHA256 or RSA using a cryptographically secure key of at least 256 bits stored in configuration.
  - Include essential claims only (`sub`, `email`, `role`, `jti`, `fullName`). Do NOT store sensitive secrets or full employee salary details in JWT claims.
  - Enforce short expiration times (e.g. 60–120 minutes) on access tokens.
- **Client Storage**: Store JWT tokens in client memory or browser `localStorage` solely for the active session; clear tokens immediately upon logout.

## 2. Authorization & Role-Based Access Control (RBAC)
- **Authoritative Backend**: Always enforce authorization at the Web API level using `[Authorize]` and `[Authorize(Roles = "...")]` attributes.
  - Frontend route guards (`AuthGuard`, `RoleGuard`) and UI hiding (`*ngIf`) are presentation-tier conveniences; they must never be treated as the security boundary.
- **Role Hierarchy**:
  - `Admin`: Full system administration, role management, user account linking, master data deletion.
  - `HR`: Employee lifecycle management, department/designation administration, Excel exports.
  - `Manager`: Read-only access to employee directory, department overviews, and subordinate details.
  - `Employee`: Self-service profile viewing and personal records only.
- **Resource Ownership Validation**: When an employee accesses `/api/employees/{id}`, verify that the caller is an Admin/HR or that the requested `id` strictly matches their linked `ApplicationUser.EmployeeId`.

## 3. Data Protection & Employee PII
- **Salary & Compensation Protection**:
  - Mask or redact employee salary details in endpoints exposed to non-authorized roles.
  - Never log salary numbers, bank accounts, or date of birth in application logs.
- **Input Validation & Sanitization**:
  - Validate all inputs on the server using DataAnnotations and model state validation before execution.
  - Protect against SQL injection by exclusively using parameterized EF Core queries. Prohibit raw concatenated SQL strings.
- **Information Disclosure**:
  - Never return stack traces, internal database table structures, or raw EF exceptions to the client in production responses.
  - Return standardized RFC 7807 problem details or sanitized user-friendly error objects.

## 4. Secret & Configuration Management
- **No Hardcoded Credentials**: Strictly forbid committing API keys, connection strings, JWT secret keys, or database passwords in source code.
- **Environment Variables & AppSettings**: Store secrets in environment variables or user secrets (`dotnet user-secrets`) during local development and secure environment vaults in production.
