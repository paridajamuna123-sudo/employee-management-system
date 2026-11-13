using Microsoft.EntityFrameworkCore;
using EmployeeManagement.API.Application.Interfaces;
using EmployeeManagement.API.Data;
using EmployeeManagement.API.DTOs;
using EmployeeManagement.API.Models;

namespace EmployeeManagement.API.Application.Services;

public class AttendanceService : IAttendanceService
{
    private readonly AppDbContext _context;

    public AttendanceService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string? Error, TodayStatusResponseDto? Status)> CheckInAsync(int employeeId, DateTime timestamp, CancellationToken cancellationToken = default)
    {
        var today = timestamp.Date;
        var existing = await _context.Attendances
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.AttendanceDate == today, cancellationToken);

        if (existing != null && existing.ClockInTime.HasValue)
        {
            return (false, "You have already clocked in for today.", null);
        }

        // Standard shift start is 09:30 AM local/UTC context
        bool isLate = timestamp.TimeOfDay > new TimeSpan(9, 30, 0);

        if (existing == null)
        {
            existing = new Attendance
            {
                EmployeeId = employeeId,
                AttendanceDate = today,
                ClockInTime = timestamp,
                Status = isLate ? "Late" : "Present",
                IsLate = isLate,
                CreatedAt = DateTime.UtcNow
            };
            _context.Attendances.Add(existing);
        }
        else
        {
            existing.ClockInTime = timestamp;
            existing.IsLate = isLate;
            existing.Status = isLate ? "Late" : "Present";
            existing.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
        var status = await GetTodayStatusAsync(employeeId, cancellationToken);
        return (true, null, status);
    }

    public async Task<(bool Success, string? Error, TodayStatusResponseDto? Status)> CheckOutAsync(int employeeId, DateTime timestamp, CancellationToken cancellationToken = default)
    {
        var today = timestamp.Date;
        var existing = await _context.Attendances
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.AttendanceDate == today, cancellationToken);

        if (existing == null || !existing.ClockInTime.HasValue)
        {
            return (false, "You must clock in before you can clock out.", null);
        }

        existing.ClockOutTime = timestamp;
        var duration = timestamp - existing.ClockInTime.Value;
        var totalHours = (decimal)Math.Round(duration.TotalHours, 2);
        if (totalHours < 0) totalHours = 0;

        existing.WorkingHours = totalHours;
        existing.OvertimeHours = totalHours > 8.0m ? totalHours - 8.0m : 0.0m;

        if (totalHours < 4.0m)
        {
            existing.Status = "HalfDay";
        }
        else if (existing.IsLate)
        {
            existing.Status = "Late";
        }
        else
        {
            existing.Status = "Present";
        }

        existing.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        var status = await GetTodayStatusAsync(employeeId, cancellationToken);
        return (true, null, status);
    }

    public async Task<TodayStatusResponseDto> GetTodayStatusAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var record = await _context.Attendances
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.AttendanceDate == today, cancellationToken);

        if (record == null)
        {
            return new TodayStatusResponseDto
            {
                HasCheckedIn = false,
                HasCheckedOut = false,
                Status = "Absent"
            };
        }

        return new TodayStatusResponseDto
        {
            HasCheckedIn = record.ClockInTime.HasValue,
            HasCheckedOut = record.ClockOutTime.HasValue,
            ClockInTime = record.ClockInTime,
            ClockOutTime = record.ClockOutTime,
            WorkingHours = record.WorkingHours,
            Status = record.Status,
            IsLate = record.IsLate,
            Remarks = record.Remarks
        };
    }

    public async Task<PagedResult<AttendanceResponseDto>> GetAttendanceHistoryAsync(int? employeeId, AttendanceQueryParameters query, CancellationToken cancellationToken = default)
    {
        var queryable = _context.Attendances
            .Include(a => a.Employee)
                .ThenInclude(e => e!.Department)
            .AsNoTracking()
            .AsQueryable();

        if (employeeId.HasValue)
        {
            queryable = queryable.Where(a => a.EmployeeId == employeeId.Value);
        }

        if (query.DepartmentId.HasValue)
        {
            queryable = queryable.Where(a => a.Employee != null && a.Employee.DepartmentId == query.DepartmentId.Value);
        }

        if (query.FromDate.HasValue)
        {
            queryable = queryable.Where(a => a.AttendanceDate >= query.FromDate.Value.Date);
        }

        if (query.ToDate.HasValue)
        {
            queryable = queryable.Where(a => a.AttendanceDate <= query.ToDate.Value.Date);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            queryable = queryable.Where(a => a.Status == query.Status);
        }

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.Trim().ToLower();
            queryable = queryable.Where(a =>
                a.Employee != null && (
                    a.Employee.FirstName.ToLower().Contains(term) ||
                    a.Employee.LastName.ToLower().Contains(term) ||
                    a.Employee.EmployeeCode.ToLower().Contains(term)
                ));
        }

        var total = await queryable.CountAsync(cancellationToken);
        var items = await queryable
            .OrderByDescending(a => a.AttendanceDate)
            .ThenBy(a => a.EmployeeId)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(a => new AttendanceResponseDto
            {
                Id = a.Id,
                EmployeeId = a.EmployeeId,
                EmployeeCode = a.Employee != null ? a.Employee.EmployeeCode : "",
                EmployeeName = a.Employee != null ? $"{a.Employee.FirstName} {a.Employee.LastName}" : "",
                DepartmentName = a.Employee != null && a.Employee.Department != null ? a.Employee.Department.DepartmentName : "N/A",
                AttendanceDate = a.AttendanceDate,
                ClockInTime = a.ClockInTime,
                ClockOutTime = a.ClockOutTime,
                WorkingHours = a.WorkingHours,
                OvertimeHours = a.OvertimeHours,
                Status = a.Status,
                IsLate = a.IsLate,
                Remarks = a.Remarks
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<AttendanceResponseDto>
        {
            Items = items,
            TotalCount = total,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }

    public async Task<(bool Success, string? Error)> SubmitCorrectionRequestAsync(int employeeId, AttendanceCorrectionRequestDto dto, CancellationToken cancellationToken = default)
    {
        var existingPending = await _context.AttendanceCorrections
            .AnyAsync(c => c.EmployeeId == employeeId && c.AttendanceDate == dto.AttendanceDate.Date && c.Status == "Pending", cancellationToken);

        if (existingPending)
        {
            return (false, "A pending correction request already exists for this date.");
        }

        var correction = new AttendanceCorrection
        {
            EmployeeId = employeeId,
            AttendanceDate = dto.AttendanceDate.Date,
            ProposedClockIn = dto.ProposedClockIn,
            ProposedClockOut = dto.ProposedClockOut,
            Reason = dto.Reason,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _context.AttendanceCorrections.Add(correction);
        await _context.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<PagedResult<AttendanceCorrectionResponseDto>> GetCorrectionRequestsAsync(int? employeeId, string? status, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var queryable = _context.AttendanceCorrections
            .Include(c => c.Employee)
            .Include(c => c.ReviewedBy)
            .AsNoTracking()
            .AsQueryable();

        if (employeeId.HasValue)
        {
            queryable = queryable.Where(c => c.EmployeeId == employeeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            queryable = queryable.Where(c => c.Status == status);
        }

        var total = await queryable.CountAsync(cancellationToken);
        var items = await queryable
            .OrderByDescending(c => c.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new AttendanceCorrectionResponseDto
            {
                Id = c.Id,
                EmployeeId = c.EmployeeId,
                EmployeeCode = c.Employee != null ? c.Employee.EmployeeCode : "",
                EmployeeName = c.Employee != null ? $"{c.Employee.FirstName} {c.Employee.LastName}" : "",
                AttendanceDate = c.AttendanceDate,
                ProposedClockIn = c.ProposedClockIn,
                ProposedClockOut = c.ProposedClockOut,
                Reason = c.Reason,
                Status = c.Status,
                ReviewedByName = c.ReviewedBy != null ? c.ReviewedBy.FullName : null,
                ReviewRemarks = c.ReviewRemarks,
                ReviewedAt = c.ReviewedAt,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<AttendanceCorrectionResponseDto>
        {
            Items = items,
            TotalCount = total,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<(bool Success, string? Error)> ReviewCorrectionRequestAsync(int correctionId, string reviewerUserId, bool approved, string? remarks, CancellationToken cancellationToken = default)
    {
        var correction = await _context.AttendanceCorrections.FindAsync(new object[] { correctionId }, cancellationToken);
        if (correction == null) return (false, "Correction request not found.");

        if (correction.Status != "Pending") return (false, "This request has already been reviewed.");

        correction.Status = approved ? "Approved" : "Rejected";
        correction.ReviewedById = reviewerUserId;
        correction.ReviewRemarks = remarks;
        correction.ReviewedAt = DateTime.UtcNow;

        if (approved)
        {
            var attendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.EmployeeId == correction.EmployeeId && a.AttendanceDate == correction.AttendanceDate, cancellationToken);

            if (attendance == null)
            {
                attendance = new Attendance
                {
                    EmployeeId = correction.EmployeeId,
                    AttendanceDate = correction.AttendanceDate,
                    ClockInTime = correction.ProposedClockIn,
                    ClockOutTime = correction.ProposedClockOut,
                    Status = "Present",
                    Remarks = "Correction applied: " + (remarks ?? "Approved by manager"),
                    CreatedAt = DateTime.UtcNow
                };
                _context.Attendances.Add(attendance);
            }
            else
            {
                if (correction.ProposedClockIn.HasValue) attendance.ClockInTime = correction.ProposedClockIn;
                if (correction.ProposedClockOut.HasValue) attendance.ClockOutTime = correction.ProposedClockOut;
                attendance.Remarks = (attendance.Remarks != null ? attendance.Remarks + " | " : "") + "Corrected: " + (remarks ?? "Approved");
                attendance.UpdatedAt = DateTime.UtcNow;
            }

            if (attendance.ClockInTime.HasValue && attendance.ClockOutTime.HasValue)
            {
                var duration = attendance.ClockOutTime.Value - attendance.ClockInTime.Value;
                var hours = (decimal)Math.Round(duration.TotalHours, 2);
                attendance.WorkingHours = hours > 0 ? hours : 0;
                attendance.OvertimeHours = hours > 8.0m ? hours - 8.0m : 0;
                attendance.Status = hours < 4.0m ? "HalfDay" : "Present";
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> SaveManualAttendanceAsync(ManualAttendanceEntryDto dto, CancellationToken cancellationToken = default)
    {
        var employee = await _context.Employees.FindAsync(new object[] { dto.EmployeeId }, cancellationToken);
        if (employee == null) return (false, "Employee not found.");

        var record = await _context.Attendances
            .FirstOrDefaultAsync(a => a.EmployeeId == dto.EmployeeId && a.AttendanceDate == dto.AttendanceDate.Date, cancellationToken);

        decimal hours = 0;
        decimal ot = 0;
        if (dto.ClockInTime.HasValue && dto.ClockOutTime.HasValue)
        {
            var diff = dto.ClockOutTime.Value - dto.ClockInTime.Value;
            hours = (decimal)Math.Round(diff.TotalHours, 2);
            if (hours < 0) hours = 0;
            ot = hours > 8.0m ? hours - 8.0m : 0;
        }

        if (record == null)
        {
            record = new Attendance
            {
                EmployeeId = dto.EmployeeId,
                AttendanceDate = dto.AttendanceDate.Date,
                ClockInTime = dto.ClockInTime,
                ClockOutTime = dto.ClockOutTime,
                WorkingHours = hours,
                OvertimeHours = ot,
                Status = dto.Status,
                Remarks = dto.Remarks,
                CreatedAt = DateTime.UtcNow
            };
            _context.Attendances.Add(record);
        }
        else
        {
            record.ClockInTime = dto.ClockInTime;
            record.ClockOutTime = dto.ClockOutTime;
            record.WorkingHours = hours;
            record.OvertimeHours = ot;
            record.Status = dto.Status;
            record.Remarks = dto.Remarks;
            record.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return (true, null);
    }
}
