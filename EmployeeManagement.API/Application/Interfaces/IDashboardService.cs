using EmployeeManagement.API.DTOs;

namespace EmployeeManagement.API.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
    Task<List<RecentEmployeeDto>> GetRecentEmployeesAsync(int count = 5, CancellationToken cancellationToken = default);
}
