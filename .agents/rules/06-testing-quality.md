---
trigger: always_on
description: Quality assurance, build verification, testing protocols, and regression prevention rules.
---

# Testing & Quality Assurance Standards

## 1. Truth in Verification
- **Evidence-Based Reporting**: Never state or claim that a build, test, or API endpoint works unless it was actually executed, measured, and verified with a successful exit code (`0`).
- **No Mock Testing Assumptions**: If changes touch backend C# code or frontend Angular code, both must be explicitly built and verified against their respective runtimes before concluding a turn.

## 2. Mandatory Verification Protocol
Whenever code modifications occur:
1. **Backend Verification**:
   - Run `dotnet build` from `EmployeeManagement.API` and verify 0 errors and 0 new warnings.
   - Run any unit/integration tests with `dotnet test`.
2. **Frontend Verification**:
   - Run `npx ng build` from `employee-management-ui` and confirm successful compilation.
   - Run `npm test -- --watch=false --browsers=ChromeHeadless` if unit tests are configured.
3. **End-to-End API Checks**:
   - Verify modified endpoints using authenticated HTTP requests (e.g. testing JWT login, status responses, and schema contracts).

## 3. Code Cleanliness & Hygiene
- **No Orphaned Debug Artifacts**: Remove all temporary `console.log`, `debugger`, commented-out legacy blocks, and ad-hoc print statements before finalizing work.
- **Compiler Warnings**: Treat compiler warnings seriously. Resolve nullability warnings, deprecated API notices, and unused variable warnings.
- **Consistent Code Formatting**:
  - Follow standard C# coding conventions (standard indentation, braces on new lines, file-scoped namespaces where used).
  - Follow Angular/TypeScript style guidelines and enforce clean linting.

## 4. Regression Prevention Checklist
Before completing any sprint or feature task:
- [ ] Existing employee records and IDs remain intact.
- [ ] Existing JWT login and logout flows continue functioning without breaking changes.
- [ ] Role-based access controls remain enforced on both API and UI.
- [ ] Pagination, sorting, and search on lists remain functional.
- [ ] Database integrity constraints (unique codes, restrict delete on FKs) remain intact.
