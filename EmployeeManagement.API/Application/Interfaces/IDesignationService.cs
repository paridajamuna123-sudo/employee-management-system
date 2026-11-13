using EmployeeManagement.API.DTOs;
using EmployeeManagement.API.Models;

namespace EmployeeManagement.API.Application.Interfaces;

public interface IDesignationService
{
    Task<PagedResult<DesignationResponseDto>> GetDesignationsAsync(OrgQueryParameters query, CancellationToken cancellationToken = default);
    Task<DesignationResponseDto?> GetDesignationByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error, Designation? Data)> CreateDesignationAsync(DesignationDto dto, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error, Designation? Data)> UpdateDesignationAsync(int id, DesignationDto dto, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> DeleteDesignationAsync(int id, CancellationToken cancellationToken = default);
}
