import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HrOperationsService } from '../../services/hr-operations.service';
import { AuthService } from '../../services/auth.service';
import { OrgService } from '../../services/org.service';
import { TodayStatusResponse, AttendanceRecord, AttendanceCorrection } from '../../models/hr-operations';
import { Department } from '../../models/employee';

@Component({
  selector: 'app-attendance',
  templateUrl: './attendance.component.html',
  styleUrls: ['./attendance.component.scss']
})
export class AttendanceComponent implements OnInit {
  activeTab: 'my-attendance' | 'register' | 'corrections' = 'my-attendance';

  // Today Clock-in
  todayStatus: TodayStatusResponse | null = null;
  isClocking = false;
  currentTimeString = '';

  // My History
  myRecords: AttendanceRecord[] = [];
  myPage = 1;
  myTotal = 0;

  // Admin/Manager Register
  registerRecords: AttendanceRecord[] = [];
  registerTotal = 0;
  regFilter = {
    pageNumber: 1,
    pageSize: 10,
    fromDate: '',
    toDate: '',
    departmentId: '',
    status: '',
    searchTerm: ''
  };
  departments: Department[] = [];

  // Correction Request Modal
  showCorrectionModal = false;
  correctionForm: FormGroup;
  correctionsList: AttendanceCorrection[] = [];

  // Manual Entry Modal (Admin/HR)
  showManualModal = false;
  manualForm: FormGroup;

  isLoading = false;
  successMessage = '';
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private hrService: HrOperationsService,
    public authService: AuthService,
    private orgService: OrgService
  ) {
    this.correctionForm = this.fb.group({
      attendanceDate: ['', Validators.required],
      proposedClockIn: [''],
      proposedClockOut: [''],
      reason: ['', [Validators.required, Validators.maxLength(500)]]
    });

    this.manualForm = this.fb.group({
      employeeId: ['', Validators.required],
      attendanceDate: [new Date().toISOString().substring(0, 10), Validators.required],
      clockInTime: [''],
      clockOutTime: [''],
      status: ['Present', Validators.required],
      remarks: ['']
    });
  }

  ngOnInit(): void {
    this.updateClock();
    setInterval(() => this.updateClock(), 1000);

    this.loadTodayStatus();
    this.loadMyHistory();

    if (this.canManageAttendance()) {
      this.loadDepartments();
      this.loadRegister();
    }
  }

  updateClock(): void {
    const now = new Date();
    this.currentTimeString = now.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', second: '2-digit' });
  }

  canManageAttendance(): boolean {
    return this.authService.hasAnyRole(['Admin', 'HR', 'Manager']);
  }

  loadTodayStatus(): void {
    this.hrService.getTodayStatus().subscribe({
      next: (res: TodayStatusResponse) => this.todayStatus = res,
      error: () => {}
    });
  }

  clockIn(): void {
    this.isClocking = true;
    this.hrService.checkIn().subscribe({
      next: (res: TodayStatusResponse) => {
        this.todayStatus = res;
        this.isClocking = false;
        this.successMessage = 'Successfully clocked in for today!';
        this.loadMyHistory();
      },
      error: (err: any) => {
        this.isClocking = false;
        this.errorMessage = err.error?.message || 'Failed to clock in.';
      }
    });
  }

  clockOut(): void {
    this.isClocking = true;
    this.hrService.checkOut().subscribe({
      next: (res: TodayStatusResponse) => {
        this.todayStatus = res;
        this.isClocking = false;
        this.successMessage = 'Successfully clocked out. Have a great day!';
        this.loadMyHistory();
      },
      error: (err: any) => {
        this.isClocking = false;
        this.errorMessage = err.error?.message || 'Failed to clock out.';
      }
    });
  }

  loadMyHistory(): void {
    this.hrService.getMyAttendanceHistory(this.myPage, 10).subscribe({
      next: (res: any) => {
        this.myRecords = res.items;
        this.myTotal = res.totalCount;
      },
      error: () => {}
    });
  }

  loadDepartments(): void {
    this.orgService.getDepartments({ pageNumber: 1, pageSize: 50 }).subscribe({
      next: (res: any) => this.departments = res.items,
      error: () => {}
    });
  }

  loadRegister(): void {
    this.isLoading = true;
    this.hrService.getAttendanceRegister(this.regFilter).subscribe({
      next: (res: any) => {
        this.registerRecords = res.items;
        this.registerTotal = res.totalCount;
        this.isLoading = false;
      },
      error: () => this.isLoading = false
    });
  }

  applyRegisterFilter(): void {
    this.regFilter.pageNumber = 1;
    this.loadRegister();
  }

  exportExcel(): void {
    this.hrService.exportAttendanceExcel(
      this.regFilter.fromDate,
      this.regFilter.toDate,
      this.regFilter.departmentId ? +this.regFilter.departmentId : undefined
    ).subscribe((blob: Blob) => {
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `Attendance_Register_${new Date().toISOString().substring(0, 10)}.xlsx`;
      a.click();
      window.URL.revokeObjectURL(url);
    });
  }

  openCorrectionModal(): void {
    this.correctionForm.reset({
      attendanceDate: new Date().toISOString().substring(0, 10),
      reason: ''
    });
    this.showCorrectionModal = true;
  }

  submitCorrection(): void {
    if (this.correctionForm.invalid) return;
    this.hrService.submitCorrection(this.correctionForm.value).subscribe({
      next: () => {
        this.successMessage = 'Correction request submitted for approval.';
        this.showCorrectionModal = false;
        this.loadCorrections();
      },
      error: (err: any) => {
        this.errorMessage = err.error?.message || 'Error submitting correction request.';
      }
    });
  }

  loadCorrections(): void {
    this.hrService.getCorrections().subscribe({
      next: (res: any) => this.correctionsList = res.items,
      error: () => {}
    });
  }

  reviewCorrection(id: number, approved: boolean): void {
    const remarks = prompt(`Enter optional remarks for ${approved ? 'approval' : 'rejection'}:`);
    this.hrService.reviewCorrection(id, approved, remarks || undefined).subscribe({
      next: () => {
        this.successMessage = `Correction request ${approved ? 'approved' : 'rejected'}.`;
        this.loadCorrections();
        if (this.canManageAttendance()) this.loadRegister();
      },
      error: (err: any) => this.errorMessage = err.error?.message || 'Error reviewing request.'
    });
  }
}
