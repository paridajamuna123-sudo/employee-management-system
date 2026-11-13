using Microsoft.EntityFrameworkCore;
using EmployeeManagement.API.Application.Interfaces;
using EmployeeManagement.API.Data;
using EmployeeManagement.API.DTOs;
using EmployeeManagement.API.Models;

namespace EmployeeManagement.API.Application.Services;

public class LeaveService : ILeaveService
{
    private readonly AppDbContext _context;

    public LeaveService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<LeaveTypeDto>> GetLeaveTypesAsync(bool onlyActive = true, CancellationToken cancellationToken = default)
    {
        var query = _context.LeaveTypes.AsNoTracking();
        if (onlyActive) query = query.Where(lt => lt.IsActive);

        return await query
            .OrderBy(lt => lt.Name)
            .Select(lt => new LeaveTypeDto
            {
                Id = lt.Id,
                Name = lt.Name,
                Code = lt.Code,
                Description = lt.Description,
                DefaultDaysPerYear = lt.DefaultDaysPerYear,
                IsPaid = lt.IsPaid,
                RequiresDocument = lt.RequiresDocument,
                IsActive = lt.IsActive
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<(bool Success, string? Error, LeaveTypeDto? Data)> CreateLeaveTypeAsync(LeaveTypeDto dto, CancellationToken cancellationToken = default)
    {
        var code = dto.Code.Trim().ToUpper();
        if (await _context.LeaveTypes.AnyAsync(lt => lt.Code.ToUpper() == code, cancellationToken))
        {
            return (false, $"Leave type code '{code}' already exists.", null);
        }

        var leaveType = new LeaveType
        {
            Name = dto.Name.Trim(),
            Code = code,
            Description = dto.Description,
            DefaultDaysPerYear = dto.DefaultDaysPerYear,
            IsPaid = dto.IsPaid,
            RequiresDocument = dto.RequiresDocument,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.LeaveTypes.Add(leaveType);
        await _context.SaveChangesAsync(cancellationToken);

        // Auto-allocate initial balances for existing active employees
        if (leaveType.DefaultDaysPerYear > 0)
        {
            var year = DateTime.UtcNow.Year;
            var activeEmployees = await _context.Employees.Where(e => e.IsActive).Select(e => e.Id).ToListAsync(cancellationToken);
            foreach (var empId in activeEmployees)
            {
                _context.LeaveBalances.Add(new LeaveBalance
                {
                    EmployeeId = empId,
                    LeaveTypeId = leaveType.Id,
                    Year = year,
                    TotalDays = leaveType.DefaultDaysPerYear,
                    UsedDays = 0,
                    RemainingDays = leaveType.DefaultDaysPerYear
                });
            }
            await _context.SaveChangesAsync(cancellationToken);
        }

        dto.Id = leaveType.Id;
        return (true, null, dto);
    }

    public async Task<(bool Success, string? Error, LeaveTypeDto? Data)> UpdateLeaveTypeAsync(int id, LeaveTypeDto dto, CancellationToken cancellationToken = default)
    {
        var leaveType = await _context.LeaveTypes.FindAsync(new object[] { id }, cancellationToken);
        if (leaveType == null) return (false, "Leave type not found.", null);

        var code = dto.Code.Trim().ToUpper();
        if (await _context.LeaveTypes.AnyAsync(lt => lt.Code.ToUpper() == code && lt.Id != id, cancellationToken))
        {
            return (false, $"Leave type code '{code}' already exists.", null);
        }

        leaveType.Name = dto.Name.Trim();
        leaveType.Code = code;
        leaveType.Description = dto.Description;
        leaveType.DefaultDaysPerYear = dto.DefaultDaysPerYear;
        leaveType.IsPaid = dto.IsPaid;
        leaveType.RequiresDocument = dto.RequiresDocument;
        leaveType.IsActive = dto.IsActive;
        leaveType.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return (true, null, dto);
    }

    public async Task<List<LeaveBalanceDto>> GetLeaveBalancesAsync(int employeeId, int year, CancellationToken cancellationToken = default)
    {
        var balances = await _context.LeaveBalances
            .Include(lb => lb.LeaveType)
            .AsNoTracking()
            .Where(lb => lb.EmployeeId == employeeId && lb.Year == year)
            .OrderBy(lb => lb.LeaveType != null ? lb.LeaveType.Name : "")
            .Select(lb => new LeaveBalanceDto
            {
                Id = lb.Id,
                LeaveTypeId = lb.LeaveTypeId,
                LeaveTypeName = lb.LeaveType != null ? lb.LeaveType.Name : "",
                LeaveTypeCode = lb.LeaveType != null ? lb.LeaveType.Code : "",
                Year = lb.Year,
                TotalDays = lb.TotalDays,
                UsedDays = lb.UsedDays,
                RemainingDays = lb.RemainingDays,
                IsPaid = lb.LeaveType != null && lb.LeaveType.IsPaid
            })
            .ToListAsync(cancellationToken);

        // If none found for current year, initialize them from active LeaveTypes
        if (!balances.Any())
        {
            var types = await _context.LeaveTypes.Where(lt => lt.IsActive).ToListAsync(cancellationToken);
            foreach (var t in types)
            {
                var newBal = new LeaveBalance
                {
                    EmployeeId = employeeId,
                    LeaveTypeId = t.Id,
                    Year = year,
                    TotalDays = t.DefaultDaysPerYear,
                    UsedDays = 0,
                    RemainingDays = t.DefaultDaysPerYear
                };
                _context.LeaveBalances.Add(newBal);
            }
            await _context.SaveChangesAsync(cancellationToken);

            return await GetLeaveBalancesAsync(employeeId, year, cancellationToken);
        }

        return balances;
    }

    public async Task<(bool Success, string? Error, int? RequestId)> ApplyLeaveAsync(int employeeId, LeaveApplyDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.StartDate.Date > dto.EndDate.Date)
        {
            return (false, "Start date cannot be after end date.", null);
        }

        decimal requestedDays;
        if (dto.IsHalfDay)
        {
            if (dto.StartDate.Date != dto.EndDate.Date)
            {
                return (false, "Half day leave must be on a single date.", null);
            }
            requestedDays = 0.5m;
        }
        else
        {
            // Business days or calendar days: (EndDate - StartDate).Days + 1
            requestedDays = (decimal)(dto.EndDate.Date - dto.StartDate.Date).TotalDays + 1;
        }

        // Check date overlap with existing active/pending requests
        var hasOverlap = await _context.LeaveRequests
            .AnyAsync(lr => lr.EmployeeId == employeeId &&
                            lr.Status != "Rejected" && lr.Status != "Cancelled" &&
                            lr.StartDate <= dto.EndDate.Date && lr.EndDate >= dto.StartDate.Date, cancellationToken);

        if (hasOverlap)
        {
            return (false, "You already have a pending or approved leave request covering these dates.", null);
        }

        // Validate Leave Balance
        var year = dto.StartDate.Year;
        var balance = await _context.LeaveBalances
            .FirstOrDefaultAsync(lb => lb.EmployeeId == employeeId && lb.LeaveTypeId == dto.LeaveTypeId && lb.Year == year, cancellationToken);

        var leaveType = await _context.LeaveTypes.FindAsync(new object[] { dto.LeaveTypeId }, cancellationToken);
        if (leaveType == null) return (false, "Invalid leave type.", null);

        // Loss of Pay (LOP) allows negative / zero quota, but paid leaves enforce balance
        if (leaveType.IsPaid)
        {
            if (balance == null || balance.RemainingDays < requestedDays)
            {
                var avail = balance != null ? balance.RemainingDays : 0;
                return (false, $"Insufficient leave balance. Requested: {requestedDays} day(s), Available: {avail} day(s).", null);
            }
        }

        var request = new LeaveRequest
        {
            EmployeeId = employeeId,
            LeaveTypeId = dto.LeaveTypeId,
            StartDate = dto.StartDate.Date,
            EndDate = dto.EndDate.Date,
            TotalDays = requestedDays,
            IsHalfDay = dto.IsHalfDay,
            Reason = dto.Reason,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _context.LeaveRequests.Add(request);
        await _context.SaveChangesAsync(cancellationToken);

        return (true, null, request.Id);
    }

    public async Task<PagedResult<LeaveRequestResponseDto>> GetLeaveRequestsAsync(LeaveQueryParameters query, CancellationToken cancellationToken = default)
    {
        var queryable = _context.LeaveRequests
            .Include(lr => lr.Employee)
                .ThenInclude(e => e!.Department)
            .Include(lr => lr.LeaveType)
            .Include(lr => lr.Approvals)
                .ThenInclude(a => a.ApproverUser)
            .AsNoTracking()
            .AsQueryable();

        if (query.EmployeeId.HasValue)
            queryable = queryable.Where(lr => lr.EmployeeId == query.EmployeeId.Value);

        if (query.DepartmentId.HasValue)
            queryable = queryable.Where(lr => lr.Employee != null && lr.Employee.DepartmentId == query.DepartmentId.Value);

        if (query.LeaveTypeId.HasValue)
            queryable = queryable.Where(lr => lr.LeaveTypeId == query.LeaveTypeId.Value);

        if (!string.IsNullOrWhiteSpace(query.Status))
            queryable = queryable.Where(lr => lr.Status == query.Status);

        if (query.Year.HasValue)
            queryable = queryable.Where(lr => lr.StartDate.Year == query.Year.Value);

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.Trim().ToLower();
            queryable = queryable.Where(lr =>
                lr.Employee != null && (
                    lr.Employee.FirstName.ToLower().Contains(term) ||
                    lr.Employee.LastName.ToLower().Contains(term) ||
                    lr.Employee.EmployeeCode.ToLower().Contains(term) ||
                    lr.Reason.ToLower().Contains(term)
                ));
        }

        var total = await queryable.CountAsync(cancellationToken);
        var items = await queryable
            .OrderByDescending(lr => lr.CreatedAt)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(lr => new LeaveRequestResponseDto
            {
                Id = lr.Id,
                EmployeeId = lr.EmployeeId,
                EmployeeCode = lr.Employee != null ? lr.Employee.EmployeeCode : "",
                EmployeeName = lr.Employee != null ? $"{lr.Employee.FirstName} {lr.Employee.LastName}" : "",
                DepartmentName = lr.Employee != null && lr.Employee.Department != null ? lr.Employee.Department.DepartmentName : "N/A",
                LeaveTypeId = lr.LeaveTypeId,
                LeaveTypeName = lr.LeaveType != null ? lr.LeaveType.Name : "",
                LeaveTypeCode = lr.LeaveType != null ? lr.LeaveType.Code : "",
                StartDate = lr.StartDate,
                EndDate = lr.EndDate,
                TotalDays = lr.TotalDays,
                IsHalfDay = lr.IsHalfDay,
                Reason = lr.Reason,
                Status = lr.Status,
                CreatedAt = lr.CreatedAt,
                Approvals = lr.Approvals.Select(a => new LeaveApprovalHistoryDto
                {
                    ApproverName = a.ApproverUser != null ? a.ApproverUser.FullName : "System",
                    Action = a.Action,
                    Comments = a.Comments,
                    ActionDate = a.ActionDate
                }).ToList()
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<LeaveRequestResponseDto>
        {
            Items = items,
            TotalCount = total,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }

    public async Task<LeaveRequestResponseDto?> GetLeaveRequestByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var lr = await _context.LeaveRequests
            .Include(l => l.Employee).ThenInclude(e => e!.Department)
            .Include(l => l.LeaveType)
            .Include(l => l.Approvals).ThenInclude(a => a.ApproverUser)
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

        if (lr == null) return null;

        return new LeaveRequestResponseDto
        {
            Id = lr.Id,
            EmployeeId = lr.EmployeeId,
            EmployeeCode = lr.Employee != null ? lr.Employee.EmployeeCode : "",
            EmployeeName = lr.Employee != null ? $"{lr.Employee.FirstName} {lr.Employee.LastName}" : "",
            DepartmentName = lr.Employee != null && lr.Employee.Department != null ? lr.Employee.Department.DepartmentName : "N/A",
            LeaveTypeId = lr.LeaveTypeId,
            LeaveTypeName = lr.LeaveType != null ? lr.LeaveType.Name : "",
            LeaveTypeCode = lr.LeaveType != null ? lr.LeaveType.Code : "",
            StartDate = lr.StartDate,
            EndDate = lr.EndDate,
            TotalDays = lr.TotalDays,
            IsHalfDay = lr.IsHalfDay,
            Reason = lr.Reason,
            Status = lr.Status,
            CreatedAt = lr.CreatedAt,
            Approvals = lr.Approvals.Select(a => new LeaveApprovalHistoryDto
            {
                ApproverName = a.ApproverUser != null ? a.ApproverUser.FullName : "System",
                Action = a.Action,
                Comments = a.Comments,
                ActionDate = a.ActionDate
            }).ToList()
        };
    }

    public async Task<(bool Success, string? Error)> ReviewLeaveRequestAsync(int leaveRequestId, string approverUserId, string action, string? comments, CancellationToken cancellationToken = default)
    {
        var request = await _context.LeaveRequests
            .Include(lr => lr.LeaveType)
            .FirstOrDefaultAsync(lr => lr.Id == leaveRequestId, cancellationToken);

        if (request == null) return (false, "Leave request not found.");
        if (request.Status != "Pending") return (false, "Only pending leave requests can be reviewed.");

        var normalizedAction = action.Equals("Approved", StringComparison.OrdinalIgnoreCase) ? "Approved" : "Rejected";
        request.Status = normalizedAction;
        request.UpdatedAt = DateTime.UtcNow;

        _context.LeaveApprovals.Add(new LeaveApproval
        {
            LeaveRequestId = leaveRequestId,
            ApproverUserId = approverUserId,
            Action = normalizedAction,
            Comments = comments,
            ActionDate = DateTime.UtcNow
        });

        // Deduct from Leave Balance if Approved and leave is paid
        if (normalizedAction == "Approved")
        {
            var year = request.StartDate.Year;
            var balance = await _context.LeaveBalances
                .FirstOrDefaultAsync(lb => lb.EmployeeId == request.EmployeeId && lb.LeaveTypeId == request.LeaveTypeId && lb.Year == year, cancellationToken);

            if (balance != null)
            {
                balance.UsedDays += request.TotalDays;
                balance.RemainingDays = balance.TotalDays - balance.UsedDays;
                balance.UpdatedAt = DateTime.UtcNow;
            }

            // Sync with Attendance: Mark attendance dates as 'OnLeave'
            for (var d = request.StartDate.Date; d <= request.EndDate.Date; d = d.AddDays(1))
            {
                var att = await _context.Attendances
                    .FirstOrDefaultAsync(a => a.EmployeeId == request.EmployeeId && a.AttendanceDate == d, cancellationToken);

                if (att == null)
                {
                    _context.Attendances.Add(new Attendance
                    {
                        EmployeeId = request.EmployeeId,
                        AttendanceDate = d,
                        Status = request.IsHalfDay ? "HalfDay" : "OnLeave",
                        Remarks = $"Leave: {request.LeaveType?.Name ?? "Approved Leave"}",
                        CreatedAt = DateTime.UtcNow
                    });
                }
                else
                {
                    att.Status = request.IsHalfDay ? "HalfDay" : "OnLeave";
                    att.Remarks = (att.Remarks != null ? att.Remarks + " | " : "") + $"Leave: {request.LeaveType?.Name}";
                    att.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> CancelLeaveRequestAsync(int leaveRequestId, int employeeId, CancellationToken cancellationToken = default)
    {
        var request = await _context.LeaveRequests
            .Include(lr => lr.LeaveType)
            .FirstOrDefaultAsync(lr => lr.Id == leaveRequestId && lr.EmployeeId == employeeId, cancellationToken);

        if (request == null) return (false, "Leave request not found.");

        if (request.Status == "Cancelled" || request.Status == "Rejected")
        {
            return (false, "Request is already cancelled or rejected.");
        }

        bool wasApproved = request.Status == "Approved";
        request.Status = "Cancelled";
        request.UpdatedAt = DateTime.UtcNow;

        // If previously approved, refund balance
        if (wasApproved)
        {
            var year = request.StartDate.Year;
            var balance = await _context.LeaveBalances
                .FirstOrDefaultAsync(lb => lb.EmployeeId == employeeId && lb.LeaveTypeId == request.LeaveTypeId && lb.Year == year, cancellationToken);

            if (balance != null)
            {
                balance.UsedDays -= request.TotalDays;
                if (balance.UsedDays < 0) balance.UsedDays = 0;
                balance.RemainingDays = balance.TotalDays - balance.UsedDays;
                balance.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return (true, null);
    }
}
