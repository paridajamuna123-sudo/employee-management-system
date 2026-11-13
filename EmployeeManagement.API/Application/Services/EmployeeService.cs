using System.Security.Claims;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EmployeeManagement.API.Application.Interfaces;
using EmployeeManagement.API.Data;
using EmployeeManagement.API.DTOs;
using EmployeeManagement.API.Models;

namespace EmployeeManagement.API.Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public EmployeeService(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<PagedResult<EmployeeResponseDto>> GetEmployeesAsync(EmployeeQueryParameters query, CancellationToken cancellationToken = default)
    {
        var queryable = _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .AsNoTracking()
            .AsQueryable();

        // Search Filter
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.Trim().ToLower();
            queryable = queryable.Where(e =>
                e.EmployeeCode.ToLower().Contains(term) ||
                e.FirstName.ToLower().Contains(term) ||
                e.LastName.ToLower().Contains(term) ||
                e.Email.ToLower().Contains(term) ||
                (e.PhoneNumber != null && e.PhoneNumber.Contains(term))
            );
        }

        // Department Filter
        if (query.DepartmentId.HasValue)
        {
            queryable = queryable.Where(e => e.DepartmentId == query.DepartmentId.Value);
        }

        // Designation Filter
        if (query.DesignationId.HasValue)
        {
            queryable = queryable.Where(e => e.DesignationId == query.DesignationId.Value);
        }

        // Employment Type Filter
        if (!string.IsNullOrWhiteSpace(query.EmploymentType))
        {
            queryable = queryable.Where(e => e.EmploymentType == query.EmploymentType);
        }

        // Active Status Filter
        if (query.IsActive.HasValue)
        {
            queryable = queryable.Where(e => e.IsActive == query.IsActive.Value);
        }

        // Sorting
        var isDesc = string.Equals(query.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        queryable = (query.SortBy?.ToLower()) switch
        {
            "employeecode" => isDesc ? queryable.OrderByDescending(e => e.EmployeeCode) : queryable.OrderBy(e => e.EmployeeCode),
            "firstname" => isDesc ? queryable.OrderByDescending(e => e.FirstName) : queryable.OrderBy(e => e.FirstName),
            "lastname" => isDesc ? queryable.OrderByDescending(e => e.LastName) : queryable.OrderBy(e => e.LastName),
            "email" => isDesc ? queryable.OrderByDescending(e => e.Email) : queryable.OrderBy(e => e.Email),
            "department" => isDesc ? queryable.OrderByDescending(e => e.Department != null ? e.Department.DepartmentName : "") : queryable.OrderBy(e => e.Department != null ? e.Department.DepartmentName : ""),
            "designation" => isDesc ? queryable.OrderByDescending(e => e.Designation != null ? e.Designation.DesignationName : "") : queryable.OrderBy(e => e.Designation != null ? e.Designation.DesignationName : ""),
            "salary" => isDesc ? queryable.OrderByDescending(e => e.Salary) : queryable.OrderBy(e => e.Salary),
            "dateofjoining" => isDesc ? queryable.OrderByDescending(e => e.DateOfJoining) : queryable.OrderBy(e => e.DateOfJoining),
            _ => isDesc ? queryable.OrderByDescending(e => e.Id) : queryable.OrderBy(e => e.Id)
        };

        var totalCount = await queryable.CountAsync(cancellationToken);
        var items = await queryable
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(e => new EmployeeResponseDto
            {
                Id = e.Id,
                EmployeeCode = e.EmployeeCode,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Email = e.Email,
                PhoneNumber = e.PhoneNumber,
                DateOfBirth = e.DateOfBirth,
                Gender = e.Gender,
                Address = e.Address,
                DepartmentId = e.DepartmentId,
                DepartmentName = e.Department != null ? e.Department.DepartmentName : e.LegacyDepartment,
                DesignationId = e.DesignationId,
                DesignationName = e.Designation != null ? e.Designation.DesignationName : null,
                Salary = e.Salary,
                DateOfJoining = e.DateOfJoining,
                EmploymentType = e.EmploymentType,
                ProfileImageUrl = e.ProfileImageUrl,
                IsActive = e.IsActive,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<EmployeeResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }

    public async Task<(byte[] FileContent, string ContentType, string FileName)> ExportEmployeesToExcelAsync(EmployeeQueryParameters query, CancellationToken cancellationToken = default)
    {
        var queryable = _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.Trim().ToLower();
            queryable = queryable.Where(e =>
                e.EmployeeCode.ToLower().Contains(term) ||
                e.FirstName.ToLower().Contains(term) ||
                e.LastName.ToLower().Contains(term) ||
                e.Email.ToLower().Contains(term));
        }

        if (query.DepartmentId.HasValue)
            queryable = queryable.Where(e => e.DepartmentId == query.DepartmentId.Value);

        if (query.DesignationId.HasValue)
            queryable = queryable.Where(e => e.DesignationId == query.DesignationId.Value);

        if (query.IsActive.HasValue)
            queryable = queryable.Where(e => e.IsActive == query.IsActive.Value);

        var employees = await queryable.OrderBy(e => e.EmployeeCode).ToListAsync(cancellationToken);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Employees");

        string[] headers = { "Employee Code", "Full Name", "Email", "Phone", "Department", "Designation", "Employment Type", "Salary", "Joining Date", "Status" };
        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
            worksheet.Cell(1, i + 1).Style.Font.Bold = true;
            worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#1E293B");
            worksheet.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
        }

        for (int row = 0; row < employees.Count; row++)
        {
            var emp = employees[row];
            int r = row + 2;
            worksheet.Cell(r, 1).Value = emp.EmployeeCode;
            worksheet.Cell(r, 2).Value = $"{emp.FirstName} {emp.LastName}";
            worksheet.Cell(r, 3).Value = emp.Email;
            worksheet.Cell(r, 4).Value = emp.PhoneNumber ?? "";
            worksheet.Cell(r, 5).Value = emp.Department?.DepartmentName ?? emp.LegacyDepartment ?? "N/A";
            worksheet.Cell(r, 6).Value = emp.Designation?.DesignationName ?? "N/A";
            worksheet.Cell(r, 7).Value = emp.EmploymentType;
            worksheet.Cell(r, 8).Value = emp.Salary;
            worksheet.Cell(r, 9).Value = emp.DateOfJoining.ToString("yyyy-MM-dd");
            worksheet.Cell(r, 10).Value = emp.IsActive ? "Active" : "Inactive";
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();
        var fileName = $"Employees_{DateTime.UtcNow:yyyyMMdd_HHmm}.xlsx";

        return (content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    public async Task<EmployeeResponseDto?> GetEmployeeByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var emp = await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (emp == null) return null;

        var linkedUser = await _userManager.Users.FirstOrDefaultAsync(u => u.EmployeeId == id, cancellationToken);

        return new EmployeeResponseDto
        {
            Id = emp.Id,
            EmployeeCode = emp.EmployeeCode,
            FirstName = emp.FirstName,
            LastName = emp.LastName,
            Email = emp.Email,
            PhoneNumber = emp.PhoneNumber,
            DateOfBirth = emp.DateOfBirth,
            Gender = emp.Gender,
            Address = emp.Address,
            DepartmentId = emp.DepartmentId,
            DepartmentName = emp.Department != null ? emp.Department.DepartmentName : emp.LegacyDepartment,
            DesignationId = emp.DesignationId,
            DesignationName = emp.Designation != null ? emp.Designation.DesignationName : null,
            Salary = emp.Salary,
            DateOfJoining = emp.DateOfJoining,
            EmploymentType = emp.EmploymentType,
            ProfileImageUrl = emp.ProfileImageUrl,
            IsActive = emp.IsActive,
            CreatedAt = emp.CreatedAt,
            UpdatedAt = emp.UpdatedAt,
            IsAccountLinked = linkedUser != null,
            LinkedUserId = linkedUser?.Id,
            LinkedUserEmail = linkedUser?.Email
        };
    }

    public async Task<(bool Success, string? Error, int? Id, string? EmployeeCode)> CreateEmployeeAsync(EmployeeDto dto, CancellationToken cancellationToken = default)
    {
        if (await _context.Employees.AnyAsync(e => e.Email == dto.Email, cancellationToken))
        {
            return (false, "An employee with this email already exists.", null, null);
        }

        string empCode = dto.EmployeeCode?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(empCode))
        {
            var lastId = await _context.Employees.MaxAsync(e => (int?)e.Id, cancellationToken) ?? 0;
            empCode = $"EMP-{(lastId + 1):D4}";
        }
        else if (await _context.Employees.AnyAsync(e => e.EmployeeCode == empCode, cancellationToken))
        {
            return (false, $"Employee code '{empCode}' is already in use.", null, null);
        }

        var employee = new Employee
        {
            EmployeeCode = empCode,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            DateOfBirth = dto.DateOfBirth,
            Gender = dto.Gender,
            Address = dto.Address,
            DepartmentId = dto.DepartmentId,
            DesignationId = dto.DesignationId,
            Salary = dto.Salary,
            DateOfJoining = dto.DateOfJoining,
            EmploymentType = dto.EmploymentType,
            ProfileImageUrl = dto.ProfileImageUrl,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync(cancellationToken);

        return (true, null, employee.Id, employee.EmployeeCode);
    }

    public async Task<(bool Success, string? Error)> UpdateEmployeeAsync(int id, EmployeeDto dto, CancellationToken cancellationToken = default)
    {
        var emp = await _context.Employees.FindAsync([id], cancellationToken);
        if (emp == null)
            return (false, "Employee not found.");

        if (await _context.Employees.AnyAsync(e => e.Email == dto.Email && e.Id != id, cancellationToken))
        {
            return (false, "Email is already taken by another employee.");
        }

        if (!string.IsNullOrWhiteSpace(dto.EmployeeCode) &&
            await _context.Employees.AnyAsync(e => e.EmployeeCode == dto.EmployeeCode && e.Id != id, cancellationToken))
        {
            return (false, $"Employee code '{dto.EmployeeCode}' is already taken.");
        }

        emp.FirstName = dto.FirstName;
        emp.LastName = dto.LastName;
        emp.Email = dto.Email;
        emp.PhoneNumber = dto.PhoneNumber;
        emp.DateOfBirth = dto.DateOfBirth;
        emp.Gender = dto.Gender;
        emp.Address = dto.Address;
        emp.DepartmentId = dto.DepartmentId;
        emp.DesignationId = dto.DesignationId;
        emp.Salary = dto.Salary;
        emp.DateOfJoining = dto.DateOfJoining;
        emp.EmploymentType = dto.EmploymentType;
        emp.ProfileImageUrl = dto.ProfileImageUrl;
        emp.IsActive = dto.IsActive;
        emp.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(dto.EmployeeCode))
        {
            emp.EmployeeCode = dto.EmployeeCode;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool Success, string? Error, bool? IsActive)> UpdateStatusAsync(int id, bool isActive, CancellationToken cancellationToken = default)
    {
        var emp = await _context.Employees.FindAsync([id], cancellationToken);
        if (emp == null)
            return (false, "Employee not found.", null);

        emp.IsActive = isActive;
        emp.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return (true, null, emp.IsActive);
    }

    public async Task<(bool Success, string? Error, string? Message)> LinkAccountAsync(int id, string userId, CancellationToken cancellationToken = default)
    {
        var employee = await _context.Employees.FindAsync([id], cancellationToken);
        if (employee == null)
            return (false, "Employee not found.", null);

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return (false, "User account not found.", null);

        var existingLink = await _userManager.Users.FirstOrDefaultAsync(u => u.EmployeeId == id && u.Id != userId, cancellationToken);
        if (existingLink != null)
        {
            return (false, $"This employee is already linked to user account '{existingLink.Email}'.", null);
        }

        user.EmployeeId = id;
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var firstErr = result.Errors.FirstOrDefault()?.Description ?? "Failed to update user account link.";
            return (false, firstErr, null);
        }

        return (true, null, $"Successfully linked user account {user.Email} to employee {employee.EmployeeCode}.");
    }

    public async Task<(bool Success, string? Error)> DeleteEmployeeAsync(int id, CancellationToken cancellationToken = default)
    {
        var emp = await _context.Employees.FindAsync([id], cancellationToken);
        if (emp == null)
            return (false, "Employee not found.");

        _context.Employees.Remove(emp);
        await _context.SaveChangesAsync(cancellationToken);

        return (true, null);
    }
}
