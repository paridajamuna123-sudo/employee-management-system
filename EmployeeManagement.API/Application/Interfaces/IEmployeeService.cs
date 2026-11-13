using EmployeeManagement.API.DTOs;
using EmployeeManagement.API.Models;

namespace EmployeeManagement.API.Application.Interfaces;

public interface IEmployeeService
{
    Task<PagedResult<EmployeeResponseDto>> GetEmployeesAsync(EmployeeQueryParameters query, CancellationToken cancellationToken = default);
    Task<(byte[] FileContent, string ContentType, string FileName)> ExportEmployeesToExcelAsync(EmployeeQueryParameters query, CancellationToken cancellationToken = default);
    Task<EmployeeResponseDto?> GetEmployeeByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error, int? Id, string? EmployeeCode)> CreateEmployeeAsync(EmployeeDto dto, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> UpdateEmployeeAsync(int id, EmployeeDto dto, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error, bool? IsActive)> UpdateStatusAsync(int id, bool isActive, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error, string? Message)> LinkAccountAsync(int id, string userId, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> DeleteEmployeeAsync(int id, CancellationToken cancellationToken = default);
}
