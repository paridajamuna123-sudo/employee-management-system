import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HrOperationsService } from '../../services/hr-operations.service';
import { AuthService } from '../../services/auth.service';
import { OrgService } from '../../services/org.service';
import { LeaveType, LeaveBalance, LeaveRequest } from '../../models/hr-operations';
import { Department } from '../../models/employee';

@Component({
  selector: 'app-leaves',
  templateUrl: './leaves.component.html',
  styleUrls: ['./leaves.component.scss']
})
export class LeavesComponent implements OnInit {
  activeTab: 'my-leaves' | 'apply' | 'approvals' | 'types' = 'my-leaves';

  leaveTypes: LeaveType[] = [];
  myBalances: LeaveBalance[] = [];
  myRequests: LeaveRequest[] = [];
  allRequests: LeaveRequest[] = [];
  departments: Department[] = [];

  applyForm: FormGroup;
  typeForm: FormGroup;

  showTypeModal = false;
  editingTypeId: number | null = null;

  isLoading = false;
  isSaving = false;
  successMessage = '';
  errorMessage = '';

  filter = {
    pageNumber: 1,
    pageSize: 10,
    departmentId: '',
    status: '',
    year: new Date().getFullYear(),
    searchTerm: ''
  };

  constructor(
    private fb: FormBuilder,
    private hrService: HrOperationsService,
    public authService: AuthService,
    private orgService: OrgService
  ) {
    this.applyForm = this.fb.group({
      leaveTypeId: ['', Validators.required],
      startDate: ['', Validators.required],
      endDate: ['', Validators.required],
      isHalfDay: [false],
      reason: ['', [Validators.required, Validators.maxLength(1000)]]
    });

    this.typeForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      code: ['', [Validators.required, Validators.maxLength(20)]],
      description: [''],
      defaultDaysPerYear: [10, [Validators.required, Validators.min(0)]],
      isPaid: [true],
      requiresDocument: [false],
      isActive: [true]
    });
  }

  ngOnInit(): void {
    this.loadLeaveTypes();
    this.loadMyBalances();
    this.loadMyRequests();

    if (this.canManageLeaves()) {
      this.loadDepartments();
      this.loadAllRequests();
    }
  }

  canManageLeaves(): boolean {
    return this.authService.hasAnyRole(['Admin', 'HR', 'Manager']);
  }

  loadLeaveTypes(): void {
    this.hrService.getLeaveTypes().subscribe({
      next: (res: LeaveType[]) => {
        this.leaveTypes = res;
        if (res.length > 0 && !this.applyForm.get('leaveTypeId')?.value) {
          this.applyForm.patchValue({ leaveTypeId: res[0].id });
        }
      },
      error: () => {}
    });
  }

  loadMyBalances(): void {
    this.hrService.getMyLeaveBalances().subscribe({
      next: (res: LeaveBalance[]) => this.myBalances = res,
      error: () => {}
    });
  }

  loadMyRequests(): void {
    this.hrService.getMyLeaveRequests().subscribe({
      next: (res: any) => this.myRequests = res.items,
      error: () => {}
    });
  }

  loadDepartments(): void {
    this.orgService.getDepartments({ pageNumber: 1, pageSize: 50 }).subscribe({
      next: (res: any) => this.departments = res.items,
      error: () => {}
    });
  }

  loadAllRequests(): void {
    this.isLoading = true;
    this.hrService.getAllLeaveRequests(this.filter).subscribe({
      next: (res: any) => {
        this.allRequests = res.items;
        this.isLoading = false;
      },
      error: () => this.isLoading = false
    });
  }

  applyLeave(): void {
    if (this.applyForm.invalid) {
      this.applyForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;
    this.errorMessage = '';
    this.hrService.applyLeave(this.applyForm.value).subscribe({
      next: () => {
        this.isSaving = false;
        this.successMessage = 'Leave application submitted successfully!';
        this.applyForm.reset({ isHalfDay: false });
        this.activeTab = 'my-leaves';
        this.loadMyRequests();
        this.loadMyBalances();
      },
      error: (err: any) => {
        this.isSaving = false;
        this.errorMessage = err.error?.message || 'Error submitting leave request.';
      }
    });
  }

  reviewRequest(id: number, action: 'Approved' | 'Rejected'): void {
    const comments = prompt(`Enter comments for ${action.toLowerCase()}:`);
    this.hrService.reviewLeaveRequest(id, action, comments || undefined).subscribe({
      next: () => {
        this.successMessage = `Leave request marked as ${action}.`;
        this.loadAllRequests();
      },
      error: (err: any) => this.errorMessage = err.error?.message || 'Error reviewing request.'
    });
  }

  cancelRequest(id: number): void {
    if (!confirm('Are you sure you want to cancel this leave request?')) return;
    this.hrService.cancelLeaveRequest(id).subscribe({
      next: () => {
        this.successMessage = 'Leave request cancelled.';
        this.loadMyRequests();
        this.loadMyBalances();
      },
      error: (err: any) => this.errorMessage = err.error?.message || 'Error cancelling request.'
    });
  }

  exportExcel(): void {
    this.hrService.exportLeavesExcel(
      this.filter.year,
      this.filter.departmentId ? +this.filter.departmentId : undefined
    ).subscribe((blob: Blob) => {
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `Leaves_Report_${this.filter.year}.xlsx`;
      a.click();
      window.URL.revokeObjectURL(url);
    });
  }
}
