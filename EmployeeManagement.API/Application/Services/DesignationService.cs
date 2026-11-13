using Microsoft.EntityFrameworkCore;
using EmployeeManagement.API.Application.Interfaces;
using EmployeeManagement.API.Data;
using EmployeeManagement.API.DTOs;
using EmployeeManagement.API.Models;

namespace EmployeeManagement.API.Application.Services;

public class DesignationService : IDesignationService
{
    private readonly AppDbContext _context;

    public DesignationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<DesignationResponseDto>> GetDesignationsAsync(OrgQueryParameters query, CancellationToken cancellationToken = default)
    {
        var queryable = _context.Designations
            .Include(d => d.Employees)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.Trim().ToLower();
            queryable = queryable.Where(d =>
                d.DesignationName.ToLower().Contains(term) ||
                (d.Description != null && d.Description.ToLower().Contains(term)));
        }

        if (query.IsActive.HasValue)
        {
            queryable = queryable.Where(d => d.IsActive == query.IsActive.Value);
        }

        var total = await queryable.CountAsync(cancellationToken);
        var items = await queryable
            .OrderBy(d => d.DesignationName)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(d => new DesignationResponseDto
            {
                Id = d.Id,
                DesignationName = d.DesignationName,
                Description = d.Description,
                IsActive = d.IsActive,
                EmployeeCount = d.Employees.Count(),
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<DesignationResponseDto>
        {
            Items = items,
            TotalCount = total,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }

    public async Task<DesignationResponseDto?> GetDesignationByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var desig = await _context.Designations
            .Include(d => d.Employees)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (desig == null) return null;

        return new DesignationResponseDto
        {
            Id = desig.Id,
            DesignationName = desig.DesignationName,
            Description = desig.Description,
            IsActive = desig.IsActive,
            EmployeeCount = desig.Employees.Count(),
            CreatedAt = desig.CreatedAt,
            UpdatedAt = desig.UpdatedAt
        };
    }

    public async Task<(bool Success, string? Error, Designation? Data)> CreateDesignationAsync(DesignationDto dto, CancellationToken cancellationToken = default)
    {
        var name = dto.DesignationName.Trim();
        if (await _context.Designations.AnyAsync(d => d.DesignationName.ToLower() == name.ToLower(), cancellationToken))
        {
            return (false, $"Designation '{name}' already exists.", null);
        }

        var desig = new Designation
        {
            DesignationName = name,
            Description = dto.Description,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.Designations.Add(desig);
        await _context.SaveChangesAsync(cancellationToken);

        return (true, null, desig);
    }

    public async Task<(bool Success, string? Error, Designation? Data)> UpdateDesignationAsync(int id, DesignationDto dto, CancellationToken cancellationToken = default)
    {
        var desig = await _context.Designations.FindAsync([id], cancellationToken);
        if (desig == null)
        {
            return (false, "Designation not found.", null);
        }

        var name = dto.DesignationName.Trim();
        if (await _context.Designations.AnyAsync(d => d.DesignationName.ToLower() == name.ToLower() && d.Id != id, cancellationToken))
        {
            return (false, $"Designation '{name}' already exists.", null);
        }

        desig.DesignationName = name;
        desig.Description = dto.Description;
        desig.IsActive = dto.IsActive;
        desig.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return (true, null, desig);
    }

    public async Task<(bool Success, string? Error)> DeleteDesignationAsync(int id, CancellationToken cancellationToken = default)
    {
        var desig = await _context.Designations
            .Include(d => d.Employees)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (desig == null)
        {
            return (false, "Designation not found.");
        }

        if (desig.Employees.Any())
        {
            return (false, $"Cannot delete designation '{desig.DesignationName}' because {desig.Employees.Count} employee(s) are currently assigned to it. Please reassign the employees first.");
        }

        _context.Designations.Remove(desig);
        await _context.SaveChangesAsync(cancellationToken);

        return (true, null);
    }
}
