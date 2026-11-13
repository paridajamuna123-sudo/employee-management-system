using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EmployeeManagement.API.Application.Interfaces;
using EmployeeManagement.API.DTOs;

namespace EmployeeManagement.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DesignationsController : ControllerBase
{
    private readonly IDesignationService _designationService;

    public DesignationsController(IDesignationService designationService)
    {
        _designationService = designationService;
    }

    // GET: api/designations
    [HttpGet]
    public async Task<IActionResult> GetDesignations([FromQuery] OrgQueryParameters query, CancellationToken cancellationToken)
    {
        var result = await _designationService.GetDesignationsAsync(query, cancellationToken);
        return Ok(result);
    }

    // GET: api/designations/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDesignation(int id, CancellationToken cancellationToken)
    {
        var desig = await _designationService.GetDesignationByIdAsync(id, cancellationToken);
        if (desig == null)
            return NotFound(new { message = "Designation not found." });

        return Ok(desig);
    }

    // POST: api/designations
    [HttpPost]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> CreateDesignation([FromBody] DesignationDto dto, CancellationToken cancellationToken)
    {
        var (success, error, desig) = await _designationService.CreateDesignationAsync(dto, cancellationToken);
        if (!success)
        {
            return Conflict(new { message = error });
        }

        return CreatedAtAction(nameof(GetDesignation), new { id = desig!.Id }, desig);
    }

    // PUT: api/designations/{id}
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> UpdateDesignation(int id, [FromBody] DesignationDto dto, CancellationToken cancellationToken)
    {
        var (success, error, desig) = await _designationService.UpdateDesignationAsync(id, dto, cancellationToken);
        if (!success)
        {
            if (error == "Designation not found.")
                return NotFound(new { message = error });

            return Conflict(new { message = error });
        }

        return Ok(desig);
    }

    // DELETE: api/designations/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> DeleteDesignation(int id, CancellationToken cancellationToken)
    {
        var (success, error) = await _designationService.DeleteDesignationAsync(id, cancellationToken);
        if (!success)
        {
            if (error == "Designation not found.")
                return NotFound(new { message = error });

            return BadRequest(new { message = error });
        }

        return Ok(new { message = "Designation deleted successfully." });
    }
}
