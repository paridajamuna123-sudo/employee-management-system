using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EmployeeManagement.API.Application.Interfaces;
using EmployeeManagement.API.DTOs;

namespace EmployeeManagement.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,HR,Manager")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    // GET /api/reports/attendance/daily
    [HttpGet("attendance/daily")]
    public async Task<IActionResult> GetDailyAttendance([FromQuery] DateTime? date, [FromQuery] int? departmentId, CancellationToken cancellationToken)
    {
        var targetDate = date ?? DateTime.UtcNow.Date;
        var report = await _reportService.GetDailyAttendanceReportAsync(targetDate, departmentId, cancellationToken);
        return Ok(report);
    }

    // GET /api/reports/attendance/monthly
    [HttpGet("attendance/monthly")]
    public async Task<IActionResult> GetMonthlyAttendance([FromQuery] int? year, [FromQuery] int? month, [FromQuery] int? departmentId, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var targetYear = year ?? now.Year;
        var targetMonth = month ?? now.Month;
        var report = await _reportService.GetMonthlyAttendanceSummaryAsync(targetYear, targetMonth, departmentId, cancellationToken);
        return Ok(report);
    }

    // GET /api/reports/attendance/export
    [HttpGet("attendance/export")]
    public async Task<IActionResult> ExportAttendanceExcel([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, [FromQuery] int? departmentId, CancellationToken cancellationToken)
    {
        var to = toDate ?? DateTime.UtcNow.Date;
        var from = fromDate ?? to.AddDays(-30);
        var (bytes, contentType, fileName) = await _reportService.ExportAttendanceReportToExcelAsync(from, to, departmentId, cancellationToken);
        return File(bytes, contentType, fileName);
    }

    // GET /api/reports/leaves/export
    [HttpGet("leaves/export")]
    public async Task<IActionResult> ExportLeavesExcel([FromQuery] int? year, [FromQuery] int? departmentId, CancellationToken cancellationToken)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        var (bytes, contentType, fileName) = await _reportService.ExportLeaveReportToExcelAsync(targetYear, departmentId, cancellationToken);
        return File(bytes, contentType, fileName);
    }
}
