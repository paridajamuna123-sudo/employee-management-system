---
trigger: glob
globs: "**/Data/**, **/Models/**, **/Migrations/**, **/*DbContext*.cs"
description: Standards for SQL Server database design, EF Core migrations, and non-destructive data handling.
---

# Database & Entity Framework Core Standards

## 1. Migration Governance & Data Safety
- **Never Modify Existing Records Destructively**: Existing employee IDs, primary keys, and historical records must be preserved at all costs.
- **Explicit Approval for Migrations**:
  - Never execute `dotnet ef database update` without a documented, verified plan.
  - Never drop existing tables, columns, or foreign keys without explicit user approval.
- **Non-Destructive Schema Evolution**:
  - When migrating string columns to relational foreign keys (e.g. `Department` string -> `DepartmentId`), retain legacy columns (`LegacyDepartment`) until data migration is fully verified.
  - Embed data backfill scripts (`migrationBuilder.Sql(...)`) inside migration files to migrate existing data seamlessly.

## 2. Entity & Relationship Design
- **Primary & Foreign Keys**:
  - Use strongly typed integer identity primary keys (`Id`) or GUIDs as designated by the existing architecture.
  - Always configure explicit Foreign Key relationships in `AppDbContext.OnModelCreating`.
- **Delete Behaviors**:
  - Strictly avoid accidental cascading deletes on core business entities. Use `DeleteBehavior.Restrict` or `DeleteBehavior.NoAction` on organizational foreign keys (`DepartmentId`, `DesignationId`) to prevent deleting parent records while employees remain assigned.
  - Implement soft deletion or active status flags (`IsActive = true/false`) instead of hard physical deletion for sensitive audit records.
- **Constraints & Indexes**:
  - Apply unique constraints and indexes to natural keys (e.g. `EmployeeCode`, `DepartmentName`, `DesignationName`).
  - Use filtered unique indexes for nullable 1:0..1 relationships (e.g. `AspNetUsers.EmployeeId IS NOT NULL`).
- **Audit Columns**:
  - Include `CreatedAt` (UTC) and `UpdatedAt` (UTC nullable) timestamps on all master entities.
  - Automatically manage timestamps during `SaveChanges` / `SaveChangesAsync` or entity mutations.

## 3. Query Performance & EF Core Best Practices
- **Explicit Projection**: Always project into DTOs using `.Select()` so that only the necessary SQL columns are requested from SQL Server.
- **Read-Only Tracking**: Always append `.AsNoTracking()` to queries that do not intend to modify entity instances.
- **Database-Side Pagination**: Always apply `.Skip()` and `.Take()` directly in the LINQ expression before calling `.ToListAsync()`.
- **Eager Loading**: Use `.Include()` / `.ThenInclude()` carefully and deliberately to avoid cartesian explosion and N+1 query loops.
- **Atomic Transactions**: For operations spanning multiple tables or relational updates (e.g., creating an employee and linking an identity user account), wrap the operations in an `IDbContextTransaction` with rollback support.
