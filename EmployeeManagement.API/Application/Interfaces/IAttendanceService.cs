using EmployeeManagement.API.DTOs;

namespace EmployeeManagement.API.Application.Interfaces;

public interface IAttendanceService
{
    Task<(bool Success, string? Error, TodayStatusResponseDto? Status)> CheckInAsync(int employeeId, DateTime timestamp, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error, TodayStatusResponseDto? Status)> CheckOutAsync(int employeeId, DateTime timestamp, CancellationToken cancellationToken = default);
    Task<TodayStatusResponseDto> GetTodayStatusAsync(int employeeId, CancellationToken cancellationToken = default);
    Task<PagedResult<AttendanceResponseDto>> GetAttendanceHistoryAsync(int? employeeId, AttendanceQueryParameters query, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> SubmitCorrectionRequestAsync(int employeeId, AttendanceCorrectionRequestDto dto, CancellationToken cancellationToken = default);
    Task<PagedResult<AttendanceCorrectionResponseDto>> GetCorrectionRequestsAsync(int? employeeId, string? status, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> ReviewCorrectionRequestAsync(int correctionId, string reviewerUserId, bool approved, string? remarks, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> SaveManualAttendanceAsync(ManualAttendanceEntryDto dto, CancellationToken cancellationToken = default);
}
