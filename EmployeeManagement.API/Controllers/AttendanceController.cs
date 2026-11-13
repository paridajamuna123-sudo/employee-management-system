using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using EmployeeManagement.API.Application.Interfaces;
using EmployeeManagement.API.DTOs;
using EmployeeManagement.API.Models;

namespace EmployeeManagement.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;
    private readonly UserManager<ApplicationUser> _userManager;

    public AttendanceController(IAttendanceService attendanceService, UserManager<ApplicationUser> userManager)
    {
        _attendanceService = attendanceService;
        _userManager = userManager;
    }

    private async Task<int?> GetCurrentEmployeeIdAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return null;
        var user = await _userManager.FindByIdAsync(userId);
        return user?.EmployeeId;
    }

    // POST /api/attendance/check-in
    [HttpPost("check-in")]
    public async Task<IActionResult> CheckIn(CancellationToken cancellationToken)
    {
        var empId = await GetCurrentEmployeeIdAsync();
        if (!empId.HasValue) return BadRequest(new { message = "Your user account is not linked to any active employee profile." });

        var (success, error, status) = await _attendanceService.CheckInAsync(empId.Value, DateTime.UtcNow, cancellationToken);
        if (!success) return BadRequest(new { message = error });

        return Ok(status);
    }

    // POST /api/attendance/check-out
    [HttpPost("check-out")]
    public async Task<IActionResult> CheckOut(CancellationToken cancellationToken)
    {
        var empId = await GetCurrentEmployeeIdAsync();
        if (!empId.HasValue) return BadRequest(new { message = "Your user account is not linked to any active employee profile." });

        var (success, error, status) = await _attendanceService.CheckOutAsync(empId.Value, DateTime.UtcNow, cancellationToken);
        if (!success) return BadRequest(new { message = error });

        return Ok(status);
    }

    // GET /api/attendance/today-status
    [HttpGet("today-status")]
    public async Task<IActionResult> GetTodayStatus(CancellationToken cancellationToken)
    {
        var empId = await GetCurrentEmployeeIdAsync();
        if (!empId.HasValue) return BadRequest(new { message = "Account not linked to an employee profile." });

        var status = await _attendanceService.GetTodayStatusAsync(empId.Value, cancellationToken);
        return Ok(status);
    }

    // GET /api/attendance/my-history
    [HttpGet("my-history")]
    public async Task<IActionResult> GetMyAttendanceHistory([FromQuery] AttendanceQueryParameters query, CancellationToken cancellationToken)
    {
        var empId = await GetCurrentEmployeeIdAsync();
        if (!empId.HasValue) return BadRequest(new { message = "Account not linked to an employee profile." });

        var history = await _attendanceService.GetAttendanceHistoryAsync(empId.Value, query, cancellationToken);
        return Ok(history);
    }

    // GET /api/attendance/register (Admin, HR, Manager)
    [HttpGet("register")]
    [Authorize(Roles = "Admin,HR,Manager")]
    public async Task<IActionResult> GetAttendanceRegister([FromQuery] AttendanceQueryParameters query, CancellationToken cancellationToken)
    {
        var result = await _attendanceService.GetAttendanceHistoryAsync(null, query, cancellationToken);
        return Ok(result);
    }

    // POST /api/attendance/corrections
    [HttpPost("corrections")]
    public async Task<IActionResult> SubmitCorrection([FromBody] AttendanceCorrectionRequestDto dto, CancellationToken cancellationToken)
    {
        var empId = await GetCurrentEmployeeIdAsync();
        if (!empId.HasValue) return BadRequest(new { message = "Account not linked to an employee profile." });

        var (success, error) = await _attendanceService.SubmitCorrectionRequestAsync(empId.Value, dto, cancellationToken);
        if (!success) return BadRequest(new { message = error });

        return Ok(new { message = "Correction request submitted successfully." });
    }

    // GET /api/attendance/corrections
    [HttpGet("corrections")]
    public async Task<IActionResult> GetCorrections([FromQuery] string? status, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        int? filterEmpId = null;
        if (User.IsInRole("Employee") && !User.IsInRole("Admin") && !User.IsInRole("HR") && !User.IsInRole("Manager"))
        {
            filterEmpId = await GetCurrentEmployeeIdAsync();
            if (!filterEmpId.HasValue) return Ok(new PagedResult<AttendanceCorrectionResponseDto>());
        }

        var result = await _attendanceService.GetCorrectionRequestsAsync(filterEmpId, status, pageNumber, pageSize, cancellationToken);
        return Ok(result);
    }

    // POST /api/attendance/corrections/{id}/review (Admin, HR, Manager)
    [HttpPost("corrections/{id}/review")]
    [Authorize(Roles = "Admin,HR,Manager")]
    public async Task<IActionResult> ReviewCorrection(int id, [FromBody] ReviewCorrectionDto dto, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var (success, error) = await _attendanceService.ReviewCorrectionRequestAsync(id, userId, dto.Approved, dto.Remarks, cancellationToken);
        if (!success) return BadRequest(new { message = error });

        return Ok(new { message = $"Correction request {(dto.Approved ? "approved" : "rejected")} successfully." });
    }

    // POST /api/attendance/manual-entry (Admin, HR)
    [HttpPost("manual-entry")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> ManualEntry([FromBody] ManualAttendanceEntryDto dto, CancellationToken cancellationToken)
    {
        var (success, error) = await _attendanceService.SaveManualAttendanceAsync(dto, cancellationToken);
        if (!success) return BadRequest(new { message = error });

        return Ok(new { message = "Attendance record saved successfully." });
    }
}
