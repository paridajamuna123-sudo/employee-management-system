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
public class LeavesController : ControllerBase
{
    private readonly ILeaveService _leaveService;
    private readonly UserManager<ApplicationUser> _userManager;

    public LeavesController(ILeaveService leaveService, UserManager<ApplicationUser> userManager)
    {
        _leaveService = leaveService;
        _userManager = userManager;
    }

    private async Task<int?> GetCurrentEmployeeIdAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return null;
        var user = await _userManager.FindByIdAsync(userId);
        return user?.EmployeeId;
    }

    // GET /api/leaves/types
    [HttpGet("types")]
    public async Task<IActionResult> GetLeaveTypes(CancellationToken cancellationToken)
    {
        var types = await _leaveService.GetLeaveTypesAsync(true, cancellationToken);
        return Ok(types);
    }

    // POST /api/leaves/types (Admin, HR)
    [HttpPost("types")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> CreateLeaveType([FromBody] LeaveTypeDto dto, CancellationToken cancellationToken)
    {
        var (success, error, data) = await _leaveService.CreateLeaveTypeAsync(dto, cancellationToken);
        if (!success) return Conflict(new { message = error });
        return Ok(data);
    }

    // PUT /api/leaves/types/{id} (Admin, HR)
    [HttpPut("types/{id}")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> UpdateLeaveType(int id, [FromBody] LeaveTypeDto dto, CancellationToken cancellationToken)
    {
        var (success, error, data) = await _leaveService.UpdateLeaveTypeAsync(id, dto, cancellationToken);
        if (!success) return Conflict(new { message = error });
        return Ok(data);
    }

    // GET /api/leaves/my-balances
    [HttpGet("my-balances")]
    public async Task<IActionResult> GetMyBalances([FromQuery] int? year, CancellationToken cancellationToken)
    {
        var empId = await GetCurrentEmployeeIdAsync();
        if (!empId.HasValue) return BadRequest(new { message = "User is not linked to an employee profile." });

        int targetYear = year ?? DateTime.UtcNow.Year;
        var balances = await _leaveService.GetLeaveBalancesAsync(empId.Value, targetYear, cancellationToken);
        return Ok(balances);
    }

    // GET /api/leaves/balances/{employeeId} (Admin, HR, Manager)
    [HttpGet("balances/{employeeId}")]
    [Authorize(Roles = "Admin,HR,Manager")]
    public async Task<IActionResult> GetEmployeeBalances(int employeeId, [FromQuery] int? year, CancellationToken cancellationToken)
    {
        int targetYear = year ?? DateTime.UtcNow.Year;
        var balances = await _leaveService.GetLeaveBalancesAsync(employeeId, targetYear, cancellationToken);
        return Ok(balances);
    }

    // POST /api/leaves/apply
    [HttpPost("apply")]
    public async Task<IActionResult> ApplyLeave([FromBody] LeaveApplyDto dto, CancellationToken cancellationToken)
    {
        var empId = await GetCurrentEmployeeIdAsync();
        if (!empId.HasValue) return BadRequest(new { message = "User is not linked to an employee profile." });

        var (success, error, reqId) = await _leaveService.ApplyLeaveAsync(empId.Value, dto, cancellationToken);
        if (!success) return BadRequest(new { message = error });

        return Ok(new { message = "Leave application submitted successfully.", id = reqId });
    }

    // GET /api/leaves/my-requests
    [HttpGet("my-requests")]
    public async Task<IActionResult> GetMyRequests([FromQuery] LeaveQueryParameters query, CancellationToken cancellationToken)
    {
        var empId = await GetCurrentEmployeeIdAsync();
        if (!empId.HasValue) return BadRequest(new { message = "User is not linked to an employee profile." });

        query.EmployeeId = empId.Value;
        var result = await _leaveService.GetLeaveRequestsAsync(query, cancellationToken);
        return Ok(result);
    }

    // GET /api/leaves/requests (Admin, HR, Manager)
    [HttpGet("requests")]
    [Authorize(Roles = "Admin,HR,Manager")]
    public async Task<IActionResult> GetAllRequests([FromQuery] LeaveQueryParameters query, CancellationToken cancellationToken)
    {
        var result = await _leaveService.GetLeaveRequestsAsync(query, cancellationToken);
        return Ok(result);
    }

    // GET /api/leaves/requests/{id}
    [HttpGet("requests/{id}")]
    public async Task<IActionResult> GetRequestById(int id, CancellationToken cancellationToken)
    {
        var result = await _leaveService.GetLeaveRequestByIdAsync(id, cancellationToken);
        if (result == null) return NotFound(new { message = "Leave request not found." });

        // RBAC check: Employee can only view own request
        if (User.IsInRole("Employee") && !User.IsInRole("Admin") && !User.IsInRole("HR") && !User.IsInRole("Manager"))
        {
            var empId = await GetCurrentEmployeeIdAsync();
            if (result.EmployeeId != empId) return Forbid();
        }

        return Ok(result);
    }

    // POST /api/leaves/requests/{id}/review (Admin, HR, Manager)
    [HttpPost("requests/{id}/review")]
    [Authorize(Roles = "Admin,HR,Manager")]
    public async Task<IActionResult> ReviewRequest(int id, [FromBody] ReviewLeaveDto dto, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var (success, error) = await _leaveService.ReviewLeaveRequestAsync(id, userId, dto.Action, dto.Comments, cancellationToken);
        if (!success) return BadRequest(new { message = error });

        return Ok(new { message = $"Leave request {dto.Action.ToLower()} successfully." });
    }

    // POST /api/leaves/requests/{id}/cancel
    [HttpPost("requests/{id}/cancel")]
    public async Task<IActionResult> CancelRequest(int id, CancellationToken cancellationToken)
    {
        var empId = await GetCurrentEmployeeIdAsync();
        if (!empId.HasValue) return BadRequest(new { message = "User is not linked to an employee profile." });

        var (success, error) = await _leaveService.CancelLeaveRequestAsync(id, empId.Value, cancellationToken);
        if (!success) return BadRequest(new { message = error });

        return Ok(new { message = "Leave request cancelled successfully." });
    }
}
