import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { PagedResult } from '../models/employee';
import {
  TodayStatusResponse,
  AttendanceRecord,
  AttendanceCorrection,
  LeaveType,
  LeaveBalance,
  LeaveRequest,
  DailyAttendanceReport
} from '../models/hr-operations';

@Injectable({
  providedIn: 'root'
})
export class HrOperationsService {
  private apiUrl = 'https://localhost:7043/api';

  constructor(private http: HttpClient) {}

  // 1. Attendance Management
  getTodayStatus(): Observable<TodayStatusResponse> {
    return this.http.get<TodayStatusResponse>(`${this.apiUrl}/attendance/today-status`);
  }

  checkIn(): Observable<TodayStatusResponse> {
    return this.http.post<TodayStatusResponse>(`${this.apiUrl}/attendance/check-in`, {});
  }

  checkOut(): Observable<TodayStatusResponse> {
    return this.http.post<TodayStatusResponse>(`${this.apiUrl}/attendance/check-out`, {});
  }

  getMyAttendanceHistory(pageNumber = 1, pageSize = 10, fromDate?: string, toDate?: string): Observable<PagedResult<AttendanceRecord>> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    if (fromDate) params = params.set('fromDate', fromDate);
    if (toDate) params = params.set('toDate', toDate);

    return this.http.get<PagedResult<AttendanceRecord>>(`${this.apiUrl}/attendance/my-history`, { params });
  }

  getAttendanceRegister(paramsObj: any): Observable<PagedResult<AttendanceRecord>> {
    let params = new HttpParams();
    Object.keys(paramsObj).forEach(key => {
      if (paramsObj[key] !== null && paramsObj[key] !== undefined && paramsObj[key] !== '') {
        params = params.set(key, paramsObj[key].toString());
      }
    });
    return this.http.get<PagedResult<AttendanceRecord>>(`${this.apiUrl}/attendance/register`, { params });
  }

  submitCorrection(data: { attendanceDate: string; proposedClockIn?: string; proposedClockOut?: string; reason: string }): Observable<any> {
    return this.http.post(`${this.apiUrl}/attendance/corrections`, data);
  }

  getCorrections(status?: string, pageNumber = 1, pageSize = 10): Observable<PagedResult<AttendanceCorrection>> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());
    if (status) params = params.set('status', status);

    return this.http.get<PagedResult<AttendanceCorrection>>(`${this.apiUrl}/attendance/corrections`, { params });
  }

  reviewCorrection(id: number, approved: boolean, remarks?: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/attendance/corrections/${id}/review`, { approved, remarks });
  }

  saveManualAttendance(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/attendance/manual-entry`, data);
  }

  // 2. Leave Management
  getLeaveTypes(): Observable<LeaveType[]> {
    return this.http.get<LeaveType[]>(`${this.apiUrl}/leaves/types`);
  }

  getMyLeaveBalances(year?: number): Observable<LeaveBalance[]> {
    let params = new HttpParams();
    if (year) params = params.set('year', year.toString());
    return this.http.get<LeaveBalance[]>(`${this.apiUrl}/leaves/my-balances`, { params });
  }

  applyLeave(data: { leaveTypeId: number; startDate: string; endDate: string; isHalfDay: boolean; reason: string }): Observable<any> {
    return this.http.post(`${this.apiUrl}/leaves/apply`, data);
  }

  getMyLeaveRequests(pageNumber = 1, pageSize = 10, status?: string): Observable<PagedResult<LeaveRequest>> {
    let params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());
    if (status) params = params.set('status', status);

    return this.http.get<PagedResult<LeaveRequest>>(`${this.apiUrl}/leaves/my-requests`, { params });
  }

  getAllLeaveRequests(paramsObj: any): Observable<PagedResult<LeaveRequest>> {
    let params = new HttpParams();
    Object.keys(paramsObj).forEach(key => {
      if (paramsObj[key] !== null && paramsObj[key] !== undefined && paramsObj[key] !== '') {
        params = params.set(key, paramsObj[key].toString());
      }
    });
    return this.http.get<PagedResult<LeaveRequest>>(`${this.apiUrl}/leaves/requests`, { params });
  }

  reviewLeaveRequest(id: number, action: 'Approved' | 'Rejected', comments?: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/leaves/requests/${id}/review`, { action, comments });
  }

  cancelLeaveRequest(id: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/leaves/requests/${id}/cancel`, {});
  }

  // 3. HR Reports & Exports
  getDailyAttendanceReport(date?: string, departmentId?: number): Observable<DailyAttendanceReport> {
    let params = new HttpParams();
    if (date) params = params.set('date', date);
    if (departmentId) params = params.set('departmentId', departmentId.toString());

    return this.http.get<DailyAttendanceReport>(`${this.apiUrl}/reports/attendance/daily`, { params });
  }

  exportAttendanceExcel(fromDate?: string, toDate?: string, departmentId?: number): Observable<Blob> {
    let params = new HttpParams();
    if (fromDate) params = params.set('fromDate', fromDate);
    if (toDate) params = params.set('toDate', toDate);
    if (departmentId) params = params.set('departmentId', departmentId.toString());

    return this.http.get(`${this.apiUrl}/reports/attendance/export`, {
      params,
      responseType: 'blob'
    });
  }

  exportLeavesExcel(year?: number, departmentId?: number): Observable<Blob> {
    let params = new HttpParams();
    if (year) params = params.set('year', year.toString());
    if (departmentId) params = params.set('departmentId', departmentId.toString());

    return this.http.get(`${this.apiUrl}/reports/leaves/export`, {
      params,
      responseType: 'blob'
    });
  }
}
