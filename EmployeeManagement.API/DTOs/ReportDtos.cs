namespace EmployeeManagement.API.DTOs;

public class DailyAttendanceReportDto
{
    public DateTime Date { get; set; }
    public int TotalEmployees { get; set; }
    public int PresentCount { get; set; }
    public int AbsentCount { get; set; }
    public int LateCount { get; set; }
    public int HalfDayCount { get; set; }
    public int OnLeaveCount { get; set; }
    public List<AttendanceResponseDto> Details { get; set; } = new();
}

public class MonthlyAttendanceSummaryDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public int TotalDaysInMonth { get; set; }
    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    public int HalfDays { get; set; }
    public int LeaveDays { get; set; }
    public decimal TotalHoursWorked { get; set; }
    public decimal TotalOvertimeHours { get; set; }
}

public class ReportQueryParameters
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? Year { get; set; }
    public int? Month { get; set; }
    public int? DepartmentId { get; set; }
}
