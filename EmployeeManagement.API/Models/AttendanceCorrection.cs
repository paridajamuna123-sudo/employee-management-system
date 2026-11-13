using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.API.Models;

public class AttendanceCorrection
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public DateTime AttendanceDate { get; set; }

    public DateTime? ProposedClockIn { get; set; }
    public DateTime? ProposedClockOut { get; set; }

    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

    [MaxLength(450)]
    public string? ReviewedById { get; set; }
    public ApplicationUser? ReviewedBy { get; set; }

    [MaxLength(500)]
    public string? ReviewRemarks { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
