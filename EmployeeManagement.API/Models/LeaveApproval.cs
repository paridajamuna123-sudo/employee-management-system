using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.API.Models;

public class LeaveApproval
{
    public int Id { get; set; }

    public int LeaveRequestId { get; set; }
    public LeaveRequest? LeaveRequest { get; set; }

    [Required]
    [MaxLength(450)]
    public string ApproverUserId { get; set; } = string.Empty;
    public ApplicationUser? ApproverUser { get; set; }

    [Required]
    [MaxLength(20)]
    public string Action { get; set; } = "Approved"; // Approved, Rejected, Cancelled

    [MaxLength(500)]
    public string? Comments { get; set; }

    public DateTime ActionDate { get; set; } = DateTime.UtcNow;
}
