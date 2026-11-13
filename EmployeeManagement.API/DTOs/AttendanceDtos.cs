using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.API.DTOs;

public class TodayStatusResponseDto
{
    public bool HasCheckedIn { get; set; }
    public bool HasCheckedOut { get; set; }
    public DateTime? ClockInTime { get; set; }
    public DateTime? ClockOutTime { get; set; }
    public decimal WorkingHours { get; set; }
    public string Status { get; set; } = "Absent";
    public bool IsLate { get; set; }
    public string? Remarks { get; set; }
}

public class AttendanceResponseDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public DateTime AttendanceDate { get; set; }
    public DateTime? ClockInTime { get; set; }
    public DateTime? ClockOutTime { get; set; }
    public decimal WorkingHours { get; set; }
    public decimal OvertimeHours { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsLate { get; set; }
    public string? Remarks { get; set; }
}

public class AttendanceQueryParameters
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? DepartmentId { get; set; }
    public string? Status { get; set; }
    public string? SearchTerm { get; set; }
}

public class AttendanceCorrectionRequestDto
{
    [Required]
    public DateTime AttendanceDate { get; set; }
    public DateTime? ProposedClockIn { get; set; }
    public DateTime? ProposedClockOut { get; set; }

    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}

public class AttendanceCorrectionResponseDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime AttendanceDate { get; set; }
    public DateTime? ProposedClockIn { get; set; }
    public DateTime? ProposedClockOut { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ReviewedByName { get; set; }
    public string? ReviewRemarks { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ReviewCorrectionDto
{
    public bool Approved { get; set; }

    [MaxLength(500)]
    public string? Remarks { get; set; }
}

public class ManualAttendanceEntryDto
{
    [Required]
    public int EmployeeId { get; set; }

    [Required]
    public DateTime AttendanceDate { get; set; }

    public DateTime? ClockInTime { get; set; }
    public DateTime? ClockOutTime { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Present";

    [MaxLength(500)]
    public string? Remarks { get; set; }
}
