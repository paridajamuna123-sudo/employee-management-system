using System;
using System.Collections.Generic;

namespace EmployeeManagement.API.Models;

public class Department
{
    public int Id { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation property
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
