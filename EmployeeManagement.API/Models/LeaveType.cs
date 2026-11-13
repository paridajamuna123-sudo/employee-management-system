using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.API.Models;

public class LeaveType
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty; // CL, SL, EL, LOP

    [MaxLength(500)]
    public string? Description { get; set; }

    public decimal DefaultDaysPerYear { get; set; } = 0.0m;

    public bool IsPaid { get; set; } = true;
    public bool RequiresDocument { get; set; } = false;
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<LeaveBalance> LeaveBalances { get; set; } = new List<LeaveBalance>();
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
}
