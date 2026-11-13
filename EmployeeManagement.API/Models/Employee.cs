using System;

namespace EmployeeManagement.API.Models;

public class Employee
{
    public int Id { get; set; }

    public string EmployeeCode { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public string? Address { get; set; }

    // Department Relationship
    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }

    // Legacy department string retained during migration
    public string? LegacyDepartment { get; set; }

    // Designation Relationship
    public int? DesignationId { get; set; }
    public Designation? Designation { get; set; }

    public decimal Salary { get; set; }

    public DateTime DateOfJoining { get; set; }

    public string EmploymentType { get; set; } = "Full-Time";

    public string? ProfileImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
