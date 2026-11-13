using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EmployeeManagement.API.Application.Interfaces;

namespace EmployeeManagement.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    // GET /api/dashboard/summary
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
    {
        var summary = await _dashboardService.GetSummaryAsync(cancellationToken);
        return Ok(summary);
    }

    // GET /api/dashboard/recent-employees
    [HttpGet("recent-employees")]
    public async Task<IActionResult> GetRecentEmployees([FromQuery] int count = 5, CancellationToken cancellationToken = default)
    {
        var recent = await _dashboardService.GetRecentEmployeesAsync(count, cancellationToken);
        return Ok(recent);
    }
}
