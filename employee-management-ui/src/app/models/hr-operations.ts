export interface TodayStatusResponse {
  hasCheckedIn: boolean;
  hasCheckedOut: boolean;
  clockInTime?: string;
  clockOutTime?: string;
  workingHours: number;
  status: string;
  isLate: boolean;
  remarks?: string;
}

export interface AttendanceRecord {
  id: number;
  employeeId: number;
  employeeCode: string;
  employeeName: string;
  departmentName: string;
  attendanceDate: string;
  clockInTime?: string;
  clockOutTime?: string;
  workingHours: number;
  overtimeHours: number;
  status: string;
  isLate: boolean;
  remarks?: string;
}

export interface AttendanceCorrection {
  id: number;
  employeeId: number;
  employeeCode: string;
  employeeName: string;
  attendanceDate: string;
  proposedClockIn?: string;
  proposedClockOut?: string;
  reason: string;
  status: string;
  reviewedByName?: string;
  reviewRemarks?: string;
  reviewedAt?: string;
  createdAt: string;
}

export interface LeaveType {
  id: number;
  name: string;
  code: string;
  description?: string;
  defaultDaysPerYear: number;
  isPaid: boolean;
  requiresDocument: boolean;
  isActive: boolean;
}

export interface LeaveBalance {
  id: number;
  leaveTypeId: number;
  leaveTypeName: string;
  leaveTypeCode: string;
  year: number;
  totalDays: number;
  usedDays: number;
  remainingDays: number;
  isPaid: boolean;
}

export interface LeaveRequest {
  id: number;
  employeeId: number;
  employeeCode: string;
  employeeName: string;
  departmentName: string;
  leaveTypeId: number;
  leaveTypeName: string;
  leaveTypeCode: string;
  startDate: string;
  endDate: string;
  totalDays: number;
  isHalfDay: boolean;
  reason: string;
  status: string;
  createdAt: string;
  approvals: LeaveApprovalHistory[];
}

export interface LeaveApprovalHistory {
  approverName: string;
  action: string;
  comments?: string;
  actionDate: string;
}

export interface DailyAttendanceReport {
  date: string;
  totalEmployees: number;
  presentCount: number;
  absentCount: number;
  lateCount: number;
  halfDayCount: number;
  onLeaveCount: number;
  details: AttendanceRecord[];
}
