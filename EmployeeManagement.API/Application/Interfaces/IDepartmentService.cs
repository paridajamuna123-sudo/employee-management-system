using EmployeeManagement.API.DTOs;
using EmployeeManagement.API.Models;

namespace EmployeeManagement.API.Application.Interfaces;

public interface IDepartmentService
{
    Task<PagedResult<DepartmentResponseDto>> GetDepartmentsAsync(OrgQueryParameters query, CancellationToken cancellationToken = default);
    Task<DepartmentResponseDto?> GetDepartmentByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error, Department? Data)> CreateDepartmentAsync(DepartmentDto dto, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error, Department? Data)> UpdateDepartmentAsync(int id, DepartmentDto dto, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> DeleteDepartmentAsync(int id, CancellationToken cancellationToken = default);
}
