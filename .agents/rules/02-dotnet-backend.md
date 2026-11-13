---
trigger: glob
globs: "EmployeeManagement.API/**, **/*.cs, **/*.csproj"
description: Standards for C# and ASP.NET Core Web API backend development on .NET 9.
---

# .NET Backend Engineering Standards (Clean Architecture)

## 1. Clean Architecture Layers & Responsibilities
The backend follows Clean Architecture principles organized into clear functional layers:
- **Core / Domain**:
  - Contains enterprise domain entities, value objects, domain enums, and domain exceptions.
  - Has zero external dependencies (no references to EF Core, ASP.NET Core MVC, or UI packages).
  - Directory: `Domain/Entities/` (or `Models/` aliased).
- **Application**:
  - Encapsulates use cases, application services, DTO contracts, mapping logic, and interface definitions.
  - Defines service and repository abstractions (`Interfaces/`) required by the business use cases.
  - Orchestrates business workflows, validations, and pagination.
  - Directories: `Application/DTOs/`, `Application/Interfaces/`, `Application/Services/`.
- **Infrastructure**:
  - Implements persistence, external integrations, security identity stores, and file handlers.
  - Contains `AppDbContext`, EF Core entity type configurations, migrations, database seeders, and token generators.
  - Strictly implements interfaces defined in Application/Domain; dependencies point inward.
  - Directories: `Infrastructure/Data/`, `Infrastructure/Services/`, `Infrastructure/Identity/`.
- **Presentation / API**:
  - Thin ASP.NET Core controllers and middleware handling HTTP transport only.
  - Responsible for routing, model binding, HTTP status codes, Swagger metadata, and auth attributes (`[Authorize]`).
  - No direct EF Core database queries or business orchestration inside controllers; delegate all operations to Application services/handlers.
  - Directory: `Controllers/`.

## 2. Dependency Inversion & Boundaries
- **Inward Dependency Rule**: Outer layers depend on inner layers; inner layers (Domain and Application) never depend on outer layers (Infrastructure and Presentation).
- **Service & Interface Abstractions**:
  - Every application service must implement a cohesive interface (e.g., `IEmployeeService`, `IDepartmentService`, `ITokenService`).
  - Controllers depend only on Application service abstractions injected via constructor dependency injection.
- **Dedicated DTO Contracts**:
  - Never expose domain entities across the wire.
  - Map between Domain entities and DTOs within Application services using explicit projections or mapping helpers.

## 3. API Design & HTTP Conventions
- **RESTful Endpoints**: Adhere to REST semantics (`GET` for retrieval, `POST` for creation, `PUT` for complete updates, `PATCH` for partial/status updates, `DELETE` for removal).
- **HTTP Status Codes**:
  - `200 OK` / `201 Created` / `204 No Content` for successful requests.
  - `400 Bad Request` for validation failures with clear RFC 7807 problem details or structured error payloads.
  - `401 Unauthorized` for missing/invalid credentials.
  - `403 Forbidden` for role or ownership permission violations.
  - `404 Not Found` for missing resources.
  - `409 Conflict` for unique constraint violations (e.g. duplicate email/code).
- **Standardized Pagination & Filtering**:
  - All collection queries must accept `pageNumber` (1-indexed), `pageSize`, `searchTerm`, `sortBy`, and `sortDescending`.
  - Return `PagedResult<T>` containing `items`, `totalCount`, `pageNumber`, `pageSize`, `totalPages`, `hasPreviousPage`, and `hasNextPage`.
- **Contract Stability**: Never remove or rename existing DTO properties without backwards compatibility or explicit approval.

## 4. C# Code Quality & Async Practices
- **Nullable Reference Types**: Active (`<Nullable>enable</Nullable>`). Annotate optional fields with `?` and initialize non-nullable strings (`= string.Empty;`).
- **Async/Await Correctness**:
  - Always `await` async tasks asynchronously. Strictly forbid `.Result`, `.Wait()`, or `.GetAwaiter().GetResult()` in request paths to prevent thread-pool starvation and deadlocks.
  - Pass `CancellationToken cancellationToken = default` down through controllers and services to EF Core async methods (`ToListAsync`, `FirstOrDefaultAsync`, `SaveChangesAsync`).
- **Exception Handling**:
  - Use centralized exception handling or clear `try/catch` boundaries.
  - Never suppress errors with empty `catch` blocks.
  - Log full exception details internally using `ILogger<T>`, but sanitize error messages returned to clients.
- **Naming Conventions**:
  - PascalCase for classes, interfaces (`I...`), methods, public properties, and DTOs.
  - camelCase with leading underscore (`_logger`, `_employeeService`) for private readonly fields.
  - Descriptive, intention-revealing names without cryptic abbreviations.

## 5. Query Performance & Memory Optimization
- **No N+1 Queries**: Eagerly load required relationships with `.Include()` / `.ThenInclude()` or project directly.
- **Projection (`.Select()`)**: Always use `.Select()` to map entities to DTOs in read queries so only requested columns are retrieved from SQL Server.
- **Read-Only Queries**: Apply `.AsNoTracking()` to all queries that do not modify or persist tracked entities.
- **Database Pagination**: Always apply `.Skip()` and `.Take()` at the database level before materializing collections into memory.
