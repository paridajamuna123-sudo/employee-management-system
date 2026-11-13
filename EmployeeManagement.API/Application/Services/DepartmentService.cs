using Microsoft.EntityFrameworkCore;
using EmployeeManagement.API.Application.Interfaces;
using EmployeeManagement.API.Data;
using EmployeeManagement.API.DTOs;
using EmployeeManagement.API.Models;

namespace EmployeeManagement.API.Application.Services;

public class DepartmentService : IDepartmentService
{
    private readonly AppDbContext _context;

    public DepartmentService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<DepartmentResponseDto>> GetDepartmentsAsync(OrgQueryParameters query, CancellationToken cancellationToken = default)
    {
        var queryable = _context.Departments
            .Include(d => d.Employees)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.Trim().ToLower();
            queryable = queryable.Where(d =>
                d.DepartmentName.ToLower().Contains(term) ||
                (d.Description != null && d.Description.ToLower().Contains(term)));
        }

        if (query.IsActive.HasValue)
        {
            queryable = queryable.Where(d => d.IsActive == query.IsActive.Value);
        }

        var total = await queryable.CountAsync(cancellationToken);
        var items = await queryable
            .OrderBy(d => d.DepartmentName)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(d => new DepartmentResponseDto
            {
                Id = d.Id,
                DepartmentName = d.DepartmentName,
                Description = d.Description,
                IsActive = d.IsActive,
                EmployeeCount = d.Employees.Count(),
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<DepartmentResponseDto>
        {
            Items = items,
            TotalCount = total,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }

    public async Task<DepartmentResponseDto?> GetDepartmentByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var dept = await _context.Departments
            .Include(d => d.Employees)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (dept == null) return null;

        return new DepartmentResponseDto
        {
            Id = dept.Id,
            DepartmentName = dept.DepartmentName,
            Description = dept.Description,
            IsActive = dept.IsActive,
            EmployeeCount = dept.Employees.Count(),
            CreatedAt = dept.CreatedAt,
            UpdatedAt = dept.UpdatedAt
        };
    }

    public async Task<(bool Success, string? Error, Department? Data)> CreateDepartmentAsync(DepartmentDto dto, CancellationToken cancellationToken = default)
    {
        var name = dto.DepartmentName.Trim();
        if (await _context.Departments.AnyAsync(d => d.DepartmentName.ToLower() == name.ToLower(), cancellationToken))
        {
            return (false, $"Department '{name}' already exists.", null);
        }

        var dept = new Department
        {
            DepartmentName = name,
            Description = dto.Description,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.Departments.Add(dept);
        await _context.SaveChangesAsync(cancellationToken);

        return (true, null, dept);
    }

    public async Task<(bool Success, string? Error, Department? Data)> UpdateDepartmentAsync(int id, DepartmentDto dto, CancellationToken cancellationToken = default)
    {
        var dept = await _context.Departments.FindAsync([id], cancellationToken);
        if (dept == null)
        {
            return (false, "Department not found.", null);
        }

        var name = dto.DepartmentName.Trim();
        if (await _context.Departments.AnyAsync(d => d.DepartmentName.ToLower() == name.ToLower() && d.Id != id, cancellationToken))
        {
            return (false, $"Department '{name}' already exists.", null);
        }

        dept.DepartmentName = name;
        dept.Description = dto.Description;
        dept.IsActive = dto.IsActive;
        dept.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return (true, null, dept);
    }

    public async Task<(bool Success, string? Error)> DeleteDepartmentAsync(int id, CancellationToken cancellationToken = default)
    {
        var dept = await _context.Departments
            .Include(d => d.Employees)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (dept == null)
        {
            return (false, "Department not found.");
        }

        if (dept.Employees.Any())
        {
            return (false, $"Cannot delete department '{dept.DepartmentName}' because {dept.Employees.Count} employee(s) are currently assigned to it. Please reassign the employees first.");
        }

        _context.Departments.Remove(dept);
        await _context.SaveChangesAsync(cancellationToken);

        return (true, null);
    }
}
