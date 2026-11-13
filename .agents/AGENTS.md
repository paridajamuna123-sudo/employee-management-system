# Antigravity Engineering Standards — Employee Management System (EMS)

This repository contains the permanent engineering standards, architectural constraints, and operational runbooks for the Employee Management System.

All agents, workflows, and automated tools operating on this repository must strictly adhere to the rules defined in `.agents/rules/`.

## Technology Baseline

- **Backend**: ASP.NET Core Web API on .NET 9.0 (`net9.0`)
- **ORM & Database**: Entity Framework Core 9.0 with Microsoft SQL Server
- **Authentication**: ASP.NET Core Identity with JWT (JSON Web Tokens)
- **Frontend**: Angular 11.0.9 (TypeScript 4.0.2, RxJS 6.6.0, Node.js 14 compatible)
- **UI Framework**: Bootstrap 5.3 + Bootstrap Icons (SCSS styling)

## Rule Catalog (`.agents/rules/`)

1. **`01-project-architecture.md`** (`always_on`): Global architectural guidelines, SOLID principles, clean boundaries, and backwards-compatibility standards.
2. **`02-dotnet-backend.md`** (`glob: EmployeeManagement.API/**`): C# and ASP.NET Core guidelines, REST API patterns, async best practices, thin controllers, and DTO handling.
3. **`03-angular-frontend.md`** (`glob: employee-management-ui/**`): Angular 11 development constraints, reactive forms, RxJS patterns, unsubscribe lifecycle, and RBAC UI rendering.
4. **`04-database-efcore.md`** (`glob: **/*.cs, **/Migrations/**`): SQL Server and EF Core migration standards, non-destructive backfill patterns, query performance, and indexing.
5. **`05-security-standards.md`** (`always_on`): Authentication, authorization, RBAC, input sanitization, credential handling, and employee PII/salary protection.
6. **`06-testing-quality.md`** (`always_on`): Verification standards, regression prevention, build execution protocols, and quality requirements.

## Golden Rules for Modification

1. **Inspect First**: Always inspect existing code before adding or modifying logic.
2. **Preserve Functionality**: Never break or rewrite existing working features (e.g. Sprint 1 & Sprint 2 baselines).
3. **No Incompatible Syntax**: Respect the exact framework versions installed; never introduce Angular 14+ standalone components or APIs incompatible with Angular 11 and .NET 9.
4. **Never Modify Database Schemas Unchecked**: Propose EF Core migration plans and verify backups before running migrations.
5. **Verify Real Builds**: Never claim an implementation is complete without compiling both backend (`dotnet build`) and frontend (`ng build`) successfully.
