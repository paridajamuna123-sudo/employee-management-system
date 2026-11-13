import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { EmployeeService } from '../../../services/employee.service';
import { Employee } from '../../../models/employee';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-employee-profile',
  templateUrl: './employee-profile.component.html',
  styleUrls: ['./employee-profile.component.scss']
})
export class EmployeeProfileComponent implements OnInit {
  employee: Employee | null = null;
  isLoading = true;
  errorMessage = '';
  successMessage = '';

  activeTab: 'overview' | 'employment' | 'account' = 'overview';
  linkUserIdInput = '';
  isLinking = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private employeeService: EmployeeService,
    public authService: AuthService
  ) {}

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.loadEmployee(+idParam);
    }
  }

  loadEmployee(id: number): void {
    this.isLoading = true;
    this.employeeService.getEmployeeById(id).subscribe({
      next: (emp: Employee) => {
        this.employee = emp;
        this.isLoading = false;
      },
      error: (err: any) => {
        this.errorMessage = err.error?.message || 'Unable to load employee profile.';
        this.isLoading = false;
        console.error(err);
      }
    });
  }

  toggleStatus(): void {
    if (!this.employee) return;
    const newStatus = !this.employee.isActive;
    this.employeeService.updateStatus(this.employee.id, newStatus).subscribe({
      next: () => {
        this.employee!.isActive = newStatus;
        this.successMessage = `Employee status changed to ${newStatus ? 'Active' : 'Inactive'}.`;
      },
      error: (err: any) => {
        this.errorMessage = err.error?.message || 'Error updating status.';
      }
    });
  }

  linkAccount(): void {
    if (!this.employee || !this.linkUserIdInput.trim()) return;

    this.isLinking = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.employeeService.linkAccount(this.employee.id, this.linkUserIdInput.trim()).subscribe({
      next: (res: any) => {
        this.isLinking = false;
        this.successMessage = res.message || 'Account linked successfully.';
        this.loadEmployee(this.employee!.id);
      },
      error: (err: any) => {
        this.isLinking = false;
        this.errorMessage = err.error?.message || 'Failed to link Identity user account.';
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/employees']);
  }
}
