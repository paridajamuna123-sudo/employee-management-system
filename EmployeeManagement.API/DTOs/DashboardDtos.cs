using System.Collections.Generic;

namespace EmployeeManagement.API.DTOs;

public class DashboardSummaryDto
{
    public int TotalEmployees { get; set; }
    public int ActiveEmployees { get; set; }
    public int InactiveEmployees { get; set; }
    public int NewJoinersThisMonth { get; set; }
    public decimal TotalMonthlyPayroll { get; set; }
    public decimal AverageSalary { get; set; }

    public List<ChartItemDto> DepartmentDistribution { get; set; } = new();
    public List<ChartItemDto> DesignationDistribution { get; set; } = new();
}

public class ChartItemDto
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class RecentEmployeeDto
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public DateTime DateOfJoining { get; set; }
    public bool IsActive { get; set; }
}
