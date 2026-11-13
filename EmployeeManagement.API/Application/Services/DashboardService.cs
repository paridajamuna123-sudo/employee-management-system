using Microsoft.EntityFrameworkCore;
using EmployeeManagement.API.Application.Interfaces;
using EmployeeManagement.API.Data;
using EmployeeManagement.API.DTOs;

namespace EmployeeManagement.API.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;

    public DashboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var total = await _context.Employees.CountAsync(cancellationToken);
        var active = await _context.Employees.CountAsync(e => e.IsActive, cancellationToken);
        var inactive = total - active;

        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var newJoiners = await _context.Employees.CountAsync(e => e.DateOfJoining >= startOfMonth, cancellationToken);

        var totalSalary = await _context.Employees.SumAsync(e => (decimal?)e.Salary, cancellationToken) ?? 0;
        var avgSalary = total > 0 ? totalSalary / total : 0;

        // Department Distribution
        var deptDist = await _context.Departments
            .Include(d => d.Employees)
            .Where(d => d.Employees.Any())
            .Select(d => new ChartItemDto
            {
                Name = d.DepartmentName,
                Count = d.Employees.Count()
            })
            .ToListAsync(cancellationToken);

        // Designation Distribution
        var desigDist = await _context.Designations
            .Include(d => d.Employees)
            .Where(d => d.Employees.Any())
            .Select(d => new ChartItemDto
            {
                Name = d.DesignationName,
                Count = d.Employees.Count()
            })
            .ToListAsync(cancellationToken);

        return new DashboardSummaryDto
        {
            TotalEmployees = total,
            ActiveEmployees = active,
            InactiveEmployees = inactive,
            NewJoinersThisMonth = newJoiners,
            TotalMonthlyPayroll = totalSalary,
            AverageSalary = Math.Round(avgSalary, 2),
            DepartmentDistribution = deptDist,
            DesignationDistribution = desigDist
        };
    }

    public async Task<List<RecentEmployeeDto>> GetRecentEmployeesAsync(int count = 5, CancellationToken cancellationToken = default)
    {
        count = Math.Clamp(count, 1, 20);

        return await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .AsNoTracking()
            .OrderByDescending(e => e.DateOfJoining)
            .Take(count)
            .Select(e => new RecentEmployeeDto
            {
                Id = e.Id,
                EmployeeCode = e.EmployeeCode,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Email = e.Email,
                Department = e.Department != null ? e.Department.DepartmentName : (e.LegacyDepartment ?? "N/A"),
                Designation = e.Designation != null ? e.Designation.DesignationName : "N/A",
                Salary = e.Salary,
                DateOfJoining = e.DateOfJoining,
                IsActive = e.IsActive
            })
            .ToListAsync(cancellationToken);
    }
}
