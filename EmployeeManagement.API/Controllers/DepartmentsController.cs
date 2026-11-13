using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EmployeeManagement.API.Application.Interfaces;
using EmployeeManagement.API.DTOs;

namespace EmployeeManagement.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _departmentService;

    public DepartmentsController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    // GET: api/departments
    [HttpGet]
    public async Task<IActionResult> GetDepartments([FromQuery] OrgQueryParameters query, CancellationToken cancellationToken)
    {
        var result = await _departmentService.GetDepartmentsAsync(query, cancellationToken);
        return Ok(result);
    }

    // GET: api/departments/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDepartment(int id, CancellationToken cancellationToken)
    {
        var dept = await _departmentService.GetDepartmentByIdAsync(id, cancellationToken);
        if (dept == null)
            return NotFound(new { message = "Department not found." });

        return Ok(dept);
    }

    // POST: api/departments
    [HttpPost]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> CreateDepartment([FromBody] DepartmentDto dto, CancellationToken cancellationToken)
    {
        var (success, error, dept) = await _departmentService.CreateDepartmentAsync(dto, cancellationToken);
        if (!success)
        {
            return Conflict(new { message = error });
        }

        return CreatedAtAction(nameof(GetDepartment), new { id = dept!.Id }, dept);
    }

    // PUT: api/departments/{id}
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> UpdateDepartment(int id, [FromBody] DepartmentDto dto, CancellationToken cancellationToken)
    {
        var (success, error, dept) = await _departmentService.UpdateDepartmentAsync(id, dto, cancellationToken);
        if (!success)
        {
            if (error == "Department not found.")
                return NotFound(new { message = error });

            return Conflict(new { message = error });
        }

        return Ok(dept);
    }

    // DELETE: api/departments/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> DeleteDepartment(int id, CancellationToken cancellationToken)
    {
        var (success, error) = await _departmentService.DeleteDepartmentAsync(id, cancellationToken);
        if (!success)
        {
            if (error == "Department not found.")
                return NotFound(new { message = error });

            return BadRequest(new { message = error });
        }

        return Ok(new { message = "Department deleted successfully." });
    }
}
