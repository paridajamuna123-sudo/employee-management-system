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
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly UserManager<ApplicationUser> _userManager;

    public EmployeesController(IEmployeeService employeeService, UserManager<ApplicationUser> userManager)
    {
        _employeeService = employeeService;
        _userManager = userManager;
    }

    // GET: api/employees
    [HttpGet]
    [Authorize(Roles = "Admin,HR,Manager")]
    public async Task<IActionResult> GetEmployees([FromQuery] EmployeeQueryParameters query, CancellationToken cancellationToken)
    {
        var result = await _employeeService.GetEmployeesAsync(query, cancellationToken);
        return Ok(result);
    }

    // GET: api/employees/export (Excel Export with applied filters)
    [HttpGet("export")]
    [Authorize(Roles = "Admin,HR,Manager")]
    public async Task<IActionResult> ExportEmployeesToExcel([FromQuery] EmployeeQueryParameters query, CancellationToken cancellationToken)
    {
        var (content, contentType, fileName) = await _employeeService.ExportEmployeesToExcelAsync(query, cancellationToken);
        return File(content, contentType, fileName);
    }

    // GET: api/employees/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetEmployee(int id, CancellationToken cancellationToken)
    {
        // Check Employee RBAC: An Employee role user can only view their own profile
        if (User.IsInRole("Employee") && !User.IsInRole("Admin") && !User.IsInRole("HR") && !User.IsInRole("Manager"))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _userManager.FindByIdAsync(userId!);
            if (currentUser?.EmployeeId != id)
            {
                return Forbid();
            }
        }

        var dto = await _employeeService.GetEmployeeByIdAsync(id, cancellationToken);
        if (dto == null)
            return NotFound(new { message = "Employee not found." });

        return Ok(dto);
    }

    // POST: api/employees
    [HttpPost]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> CreateEmployee([FromBody] EmployeeDto dto, CancellationToken cancellationToken)
    {
        var (success, error, id, employeeCode) = await _employeeService.CreateEmployeeAsync(dto, cancellationToken);
        if (!success)
        {
            return Conflict(new { message = error });
        }

        return CreatedAtAction(nameof(GetEmployee), new { id = id!.Value }, new { id = id.Value, employeeCode });
    }

    // PUT: api/employees/{id}
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> UpdateEmployee(int id, [FromBody] EmployeeDto dto, CancellationToken cancellationToken)
    {
        var (success, error) = await _employeeService.UpdateEmployeeAsync(id, dto, cancellationToken);
        if (!success)
        {
            if (error == "Employee not found.")
                return NotFound(new { message = error });

            return Conflict(new { message = error });
        }

        return Ok(new { message = "Employee updated successfully." });
    }

    // PATCH: api/employees/{id}/status
    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] EmployeeStatusUpdateDto dto, CancellationToken cancellationToken)
    {
        var (success, error, isActive) = await _employeeService.UpdateStatusAsync(id, dto.IsActive, cancellationToken);
        if (!success)
        {
            return NotFound(new { message = error });
        }

        return Ok(new { message = $"Employee {(dto.IsActive ? "activated" : "deactivated")} successfully.", isActive });
    }

    // POST: api/employees/{id}/link-account
    [HttpPost("{id}/link-account")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> LinkAccount(int id, [FromBody] LinkAccountDto dto, CancellationToken cancellationToken)
    {
        var (success, error, message) = await _employeeService.LinkAccountAsync(id, dto.UserId, cancellationToken);
        if (!success)
        {
            if (error == "Employee not found." || error == "User account not found.")
                return NotFound(new { message = error });

            return Conflict(new { message = error });
        }

        return Ok(new { message });
    }

    // DELETE: api/employees/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteEmployee(int id, CancellationToken cancellationToken)
    {
        var (success, error) = await _employeeService.DeleteEmployeeAsync(id, cancellationToken);
        if (!success)
        {
            return NotFound(new { message = error });
        }

        return Ok(new { message = "Employee deleted successfully." });
    }
}
