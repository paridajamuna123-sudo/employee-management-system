using System;
using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.API.DTOs;

public class EmployeeDto
{
    [MaxLength(50)]
    public string? EmployeeCode { get; set; }

    [Required]
    [MinLength(2)]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MinLength(2)]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [MaxLength(20)]
    public string? Gender { get; set; }

    [MaxLength(300)]
    public string? Address { get; set; }

    public int? DepartmentId { get; set; }

    public int? DesignationId { get; set; }

    [Range(0, 100000000)]
    public decimal Salary { get; set; }

    public DateTime DateOfJoining { get; set; }

    [MaxLength(30)]
    public string EmploymentType { get; set; } = "Full-Time";

    [MaxLength(500)]
    public string? ProfileImageUrl { get; set; }

    public bool IsActive { get; set; } = true;
}

public class EmployeeResponseDto
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Address { get; set; }
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public int? DesignationId { get; set; }
    public string? DesignationName { get; set; }
    public decimal Salary { get; set; }
    public DateTime DateOfJoining { get; set; }
    public string EmploymentType { get; set; } = "Full-Time";
    public string? ProfileImageUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Linked Identity user status
    public bool IsAccountLinked { get; set; }
    public string? LinkedUserId { get; set; }
    public string? LinkedUserEmail { get; set; }
}

public class EmployeeStatusUpdateDto
{
    public bool IsActive { get; set; }
}

public class LinkAccountDto
{
    [Required]
    public string UserId { get; set; } = string.Empty;
}
