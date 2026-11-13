using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.API.Models;

public class Attendance
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public DateTime AttendanceDate { get; set; }

    public DateTime? ClockInTime { get; set; }
    public DateTime? ClockOutTime { get; set; }

    public decimal WorkingHours { get; set; } = 0.0m;
    public decimal OvertimeHours { get; set; } = 0.0m;

    [MaxLength(20)]
    public string Status { get; set; } = "Present"; // Present, Absent, Late, HalfDay, OnLeave

    public bool IsLate { get; set; } = false;

    [MaxLength(500)]
    public string? Remarks { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
