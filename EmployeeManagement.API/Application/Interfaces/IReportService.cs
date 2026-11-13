using EmployeeManagement.API.DTOs;

namespace EmployeeManagement.API.Application.Interfaces;

public interface IReportService
{
    Task<DailyAttendanceReportDto> GetDailyAttendanceReportAsync(DateTime date, int? departmentId, CancellationToken cancellationToken = default);
    Task<List<MonthlyAttendanceSummaryDto>> GetMonthlyAttendanceSummaryAsync(int year, int month, int? departmentId, CancellationToken cancellationToken = default);
    Task<(byte[] FileContent, string ContentType, string FileName)> ExportAttendanceReportToExcelAsync(DateTime fromDate, DateTime toDate, int? departmentId, CancellationToken cancellationToken = default);
    Task<(byte[] FileContent, string ContentType, string FileName)> ExportLeaveReportToExcelAsync(int year, int? departmentId, CancellationToken cancellationToken = default);
}
