using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using EmployeeManagement.API.Application.Interfaces;
using EmployeeManagement.API.Data;
using EmployeeManagement.API.DTOs;
using EmployeeManagement.API.Models;

namespace EmployeeManagement.API.Application.Services;

public class ReportService : IReportService
{
    private readonly AppDbContext _context;

    public ReportService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DailyAttendanceReportDto> GetDailyAttendanceReportAsync(DateTime date, int? departmentId, CancellationToken cancellationToken = default)
    {
        var targetDate = date.Date;

        var employeeQuery = _context.Employees
            .Include(e => e.Department)
            .AsNoTracking()
            .Where(e => e.IsActive);

        if (departmentId.HasValue)
        {
            employeeQuery = employeeQuery.Where(e => e.DepartmentId == departmentId.Value);
        }

        var activeEmployees = await employeeQuery.ToListAsync(cancellationToken);
        var totalCount = activeEmployees.Count;

        var empIds = activeEmployees.Select(e => e.Id).ToList();
        var attendances = await _context.Attendances
            .Include(a => a.Employee).ThenInclude(e => e!.Department)
            .AsNoTracking()
            .Where(a => a.AttendanceDate == targetDate && empIds.Contains(a.EmployeeId))
            .ToListAsync(cancellationToken);

        var attMap = attendances.ToDictionary(a => a.EmployeeId);

        var details = new List<AttendanceResponseDto>();
        int present = 0, absent = 0, late = 0, halfDay = 0, onLeave = 0;

        foreach (var emp in activeEmployees)
        {
            if (attMap.TryGetValue(emp.Id, out var att))
            {
                details.Add(new AttendanceResponseDto
                {
                    Id = att.Id,
                    EmployeeId = emp.Id,
                    EmployeeCode = emp.EmployeeCode,
                    EmployeeName = $"{emp.FirstName} {emp.LastName}",
                    DepartmentName = emp.Department != null ? emp.Department.DepartmentName : "N/A",
                    AttendanceDate = targetDate,
                    ClockInTime = att.ClockInTime,
                    ClockOutTime = att.ClockOutTime,
                    WorkingHours = att.WorkingHours,
                    OvertimeHours = att.OvertimeHours,
                    Status = att.Status,
                    IsLate = att.IsLate,
                    Remarks = att.Remarks
                });

                if (att.Status == "Present") present++;
                else if (att.Status == "Late") { late++; present++; }
                else if (att.Status == "HalfDay") halfDay++;
                else if (att.Status == "OnLeave") onLeave++;
                else absent++;
            }
            else
            {
                details.Add(new AttendanceResponseDto
                {
                    Id = 0,
                    EmployeeId = emp.Id,
                    EmployeeCode = emp.EmployeeCode,
                    EmployeeName = $"{emp.FirstName} {emp.LastName}",
                    DepartmentName = emp.Department != null ? emp.Department.DepartmentName : "N/A",
                    AttendanceDate = targetDate,
                    Status = "Absent"
                });
                absent++;
            }
        }

        return new DailyAttendanceReportDto
        {
            Date = targetDate,
            TotalEmployees = totalCount,
            PresentCount = present,
            AbsentCount = absent,
            LateCount = late,
            HalfDayCount = halfDay,
            OnLeaveCount = onLeave,
            Details = details
        };
    }

    public async Task<List<MonthlyAttendanceSummaryDto>> GetMonthlyAttendanceSummaryAsync(int year, int month, int? departmentId, CancellationToken cancellationToken = default)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);
        var daysInMonth = DateTime.DaysInMonth(year, month);

        var employeeQuery = _context.Employees
            .Include(e => e.Department)
            .AsNoTracking()
            .Where(e => e.IsActive);

        if (departmentId.HasValue)
            employeeQuery = employeeQuery.Where(e => e.DepartmentId == departmentId.Value);

        var employees = await employeeQuery.ToListAsync(cancellationToken);
        var empIds = employees.Select(e => e.Id).ToList();

        var attendances = await _context.Attendances
            .AsNoTracking()
            .Where(a => a.AttendanceDate >= startDate && a.AttendanceDate <= endDate && empIds.Contains(a.EmployeeId))
            .ToListAsync(cancellationToken);

        var grouped = attendances.GroupBy(a => a.EmployeeId).ToDictionary(g => g.Key, g => g.ToList());

        var result = new List<MonthlyAttendanceSummaryDto>();

        foreach (var emp in employees)
        {
            List<Attendance> empAtts = grouped.ContainsKey(emp.Id) ? grouped[emp.Id] : new List<Attendance>();

            int present = empAtts.Count(a => a.Status == "Present" || a.Status == "Late");
            int halfDays = empAtts.Count(a => a.Status == "HalfDay");
            int leaves = empAtts.Count(a => a.Status == "OnLeave");
            int absent = daysInMonth - (present + halfDays + leaves);
            if (absent < 0) absent = 0;

            decimal hours = empAtts.Sum(a => a.WorkingHours);
            decimal ot = empAtts.Sum(a => a.OvertimeHours);

            result.Add(new MonthlyAttendanceSummaryDto
            {
                Year = year,
                Month = month,
                EmployeeId = emp.Id,
                EmployeeCode = emp.EmployeeCode,
                EmployeeName = $"{emp.FirstName} {emp.LastName}",
                DepartmentName = emp.Department != null ? emp.Department.DepartmentName : "N/A",
                TotalDaysInMonth = daysInMonth,
                PresentDays = present,
                HalfDays = halfDays,
                LeaveDays = leaves,
                AbsentDays = absent,
                TotalHoursWorked = hours,
                TotalOvertimeHours = ot
            });
        }

        return result;
    }

    public async Task<(byte[] FileContent, string ContentType, string FileName)> ExportAttendanceReportToExcelAsync(DateTime fromDate, DateTime toDate, int? departmentId, CancellationToken cancellationToken = default)
    {
        var from = fromDate.Date;
        var to = toDate.Date;

        var queryable = _context.Attendances
            .Include(a => a.Employee).ThenInclude(e => e!.Department)
            .Include(a => a.Employee).ThenInclude(e => e!.Designation)
            .AsNoTracking()
            .Where(a => a.AttendanceDate >= from && a.AttendanceDate <= to);

        if (departmentId.HasValue)
            queryable = queryable.Where(a => a.Employee != null && a.Employee.DepartmentId == departmentId.Value);

        var data = await queryable
            .OrderBy(a => a.AttendanceDate)
            .ThenBy(a => a.Employee != null ? a.Employee.EmployeeCode : "")
            .ToListAsync(cancellationToken);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Attendance Register");

        string[] headers = { "Date", "Employee Code", "Employee Name", "Department", "Designation", "Clock In", "Clock Out", "Hours", "Overtime", "Status", "Remarks" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#0F172A");
            cell.Style.Font.FontColor = XLColor.White;
        }

        for (int r = 0; r < data.Count; r++)
        {
            var item = data[r];
            int rowIdx = r + 2;
            worksheet.Cell(rowIdx, 1).Value = item.AttendanceDate.ToString("yyyy-MM-dd");
            worksheet.Cell(rowIdx, 2).Value = item.Employee?.EmployeeCode ?? "";
            worksheet.Cell(rowIdx, 3).Value = $"{item.Employee?.FirstName} {item.Employee?.LastName}";
            worksheet.Cell(rowIdx, 4).Value = item.Employee?.Department?.DepartmentName ?? "N/A";
            worksheet.Cell(rowIdx, 5).Value = item.Employee?.Designation?.DesignationName ?? "N/A";
            worksheet.Cell(rowIdx, 6).Value = item.ClockInTime.HasValue ? item.ClockInTime.Value.ToString("HH:mm:ss") : "--:--";
            worksheet.Cell(rowIdx, 7).Value = item.ClockOutTime.HasValue ? item.ClockOutTime.Value.ToString("HH:mm:ss") : "--:--";
            worksheet.Cell(rowIdx, 8).Value = item.WorkingHours;
            worksheet.Cell(rowIdx, 9).Value = item.OvertimeHours;
            worksheet.Cell(rowIdx, 10).Value = item.Status;
            worksheet.Cell(rowIdx, 11).Value = item.Remarks ?? "";
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var bytes = stream.ToArray();
        var fileName = $"Attendance_{from:yyyyMMdd}_to_{to:yyyyMMdd}.xlsx";

        return (bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    public async Task<(byte[] FileContent, string ContentType, string FileName)> ExportLeaveReportToExcelAsync(int year, int? departmentId, CancellationToken cancellationToken = default)
    {
        var queryable = _context.LeaveRequests
            .Include(lr => lr.Employee).ThenInclude(e => e!.Department)
            .Include(lr => lr.LeaveType)
            .AsNoTracking()
            .Where(lr => lr.StartDate.Year == year);

        if (departmentId.HasValue)
            queryable = queryable.Where(lr => lr.Employee != null && lr.Employee.DepartmentId == departmentId.Value);

        var data = await queryable
            .OrderByDescending(lr => lr.CreatedAt)
            .ToListAsync(cancellationToken);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add($"Leaves_{year}");

        string[] headers = { "Request ID", "Employee Code", "Employee Name", "Department", "Leave Type", "Start Date", "End Date", "Total Days", "Half Day", "Status", "Reason", "Applied On" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#0F172A");
            cell.Style.Font.FontColor = XLColor.White;
        }

        for (int r = 0; r < data.Count; r++)
        {
            var item = data[r];
            int rowIdx = r + 2;
            worksheet.Cell(rowIdx, 1).Value = item.Id;
            worksheet.Cell(rowIdx, 2).Value = item.Employee?.EmployeeCode ?? "";
            worksheet.Cell(rowIdx, 3).Value = $"{item.Employee?.FirstName} {item.Employee?.LastName}";
            worksheet.Cell(rowIdx, 4).Value = item.Employee?.Department?.DepartmentName ?? "N/A";
            worksheet.Cell(rowIdx, 5).Value = item.LeaveType?.Name ?? "";
            worksheet.Cell(rowIdx, 6).Value = item.StartDate.ToString("yyyy-MM-dd");
            worksheet.Cell(rowIdx, 7).Value = item.EndDate.ToString("yyyy-MM-dd");
            worksheet.Cell(rowIdx, 8).Value = item.TotalDays;
            worksheet.Cell(rowIdx, 9).Value = item.IsHalfDay ? "Yes" : "No";
            worksheet.Cell(rowIdx, 10).Value = item.Status;
            worksheet.Cell(rowIdx, 11).Value = item.Reason;
            worksheet.Cell(rowIdx, 12).Value = item.CreatedAt.ToString("yyyy-MM-dd HH:mm");
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var bytes = stream.ToArray();
        var fileName = $"Leave_Report_{year}.xlsx";

        return (bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
}
