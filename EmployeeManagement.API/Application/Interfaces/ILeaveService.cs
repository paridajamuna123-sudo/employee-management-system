using EmployeeManagement.API.DTOs;

namespace EmployeeManagement.API.Application.Interfaces;

public interface ILeaveService
{
    Task<List<LeaveTypeDto>> GetLeaveTypesAsync(bool onlyActive = true, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error, LeaveTypeDto? Data)> CreateLeaveTypeAsync(LeaveTypeDto dto, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error, LeaveTypeDto? Data)> UpdateLeaveTypeAsync(int id, LeaveTypeDto dto, CancellationToken cancellationToken = default);
    Task<List<LeaveBalanceDto>> GetLeaveBalancesAsync(int employeeId, int year, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error, int? RequestId)> ApplyLeaveAsync(int employeeId, LeaveApplyDto dto, CancellationToken cancellationToken = default);
    Task<PagedResult<LeaveRequestResponseDto>> GetLeaveRequestsAsync(LeaveQueryParameters query, CancellationToken cancellationToken = default);
    Task<LeaveRequestResponseDto?> GetLeaveRequestByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> ReviewLeaveRequestAsync(int leaveRequestId, string approverUserId, string action, string? comments, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> CancelLeaveRequestAsync(int leaveRequestId, int employeeId, CancellationToken cancellationToken = default);
}
