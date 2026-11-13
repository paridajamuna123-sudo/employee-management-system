namespace EmployeeManagement.API.Models;

public class LeaveBalance
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public int LeaveTypeId { get; set; }
    public LeaveType? LeaveType { get; set; }

    public int Year { get; set; }

    public decimal TotalDays { get; set; } = 0.0m;
    public decimal UsedDays { get; set; } = 0.0m;
    public decimal RemainingDays { get; set; } = 0.0m;

    public DateTime? UpdatedAt { get; set; }
}
