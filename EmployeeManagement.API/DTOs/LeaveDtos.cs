using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.API.DTOs;

public class LeaveTypeDto
{
    public int Id { get; set; }
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;
    [MaxLength(500)]
    public string? Description { get; set; }
    public decimal DefaultDaysPerYear { get; set; }
    public bool IsPaid { get; set; } = true;
    public bool RequiresDocument { get; set; } = false;
    public bool IsActive { get; set; } = true;
}

public class LeaveBalanceDto
{
    public int Id { get; set; }
    public int LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public string LeaveTypeCode { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal TotalDays { get; set; }
    public decimal UsedDays { get; set; }
    public decimal RemainingDays { get; set; }
    public bool IsPaid { get; set; }
}

public class LeaveApplyDto
{
    [Required]
    public int LeaveTypeId { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    public bool IsHalfDay { get; set; } = false;

    [Required]
    [MaxLength(1000)]
    public string Reason { get; set; } = string.Empty;
}

public class LeaveRequestResponseDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public int LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public string LeaveTypeCode { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalDays { get; set; }
    public bool IsHalfDay { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<LeaveApprovalHistoryDto> Approvals { get; set; } = new();
}

public class LeaveApprovalHistoryDto
{
    public string ApproverName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? Comments { get; set; }
    public DateTime ActionDate { get; set; }
}

public class ReviewLeaveDto
{
    [Required]
    [MaxLength(20)]
    public string Action { get; set; } = "Approved"; // Approved, Rejected

    [MaxLength(500)]
    public string? Comments { get; set; }
}

public class LeaveQueryParameters
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int? EmployeeId { get; set; }
    public int? DepartmentId { get; set; }
    public int? LeaveTypeId { get; set; }
    public string? Status { get; set; }
    public int? Year { get; set; }
    public string? SearchTerm { get; set; }
}
