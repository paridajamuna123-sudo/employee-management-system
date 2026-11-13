import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { EmployeeService } from '../../../services/employee.service';
import { OrgService } from '../../../services/org.service';
import { Department, Designation, Employee } from '../../../models/employee';

@Component({
  selector: 'app-employee-form',
  templateUrl: './employee-form.component.html',
  styleUrls: ['./employee-form.component.scss']
})
export class EmployeeFormComponent implements OnInit {
  employeeForm: FormGroup;
  employeeId: number | null = null;
  isEditMode = false;
  isLoading = false;
  isSaving = false;

  departments: Department[] = [];
  designations: Designation[] = [];

  successMessage = '';
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private employeeService: EmployeeService,
    private orgService: OrgService,
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.employeeForm = this.fb.group({
      employeeCode: [''],
      firstName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
      lastName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.email, Validators.maxLength(150)]],
      phoneNumber: ['', [Validators.pattern(/^[0-9+\-\s()]{7,20}$/)]],
      dateOfBirth: [''],
      gender: ['Male'],
      address: ['', [Validators.maxLength(300)]],
      departmentId: [null, [Validators.required]],
      designationId: [null, [Validators.required]],
      salary: [50000, [Validators.required, Validators.min(0)]],
      dateOfJoining: [new Date().toISOString().substring(0, 10), [Validators.required]],
      employmentType: ['Full-Time', [Validators.required]],
      profileImageUrl: [''],
      isActive: [true]
    });
  }

  ngOnInit(): void {
    this.loadDropdownData();
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.employeeId = +idParam;
      this.isEditMode = true;
      this.loadEmployeeDetails(this.employeeId);
    }
  }

  loadDropdownData(): void {
    this.orgService.getDepartments().subscribe({
      next: (res: any) => this.departments = res.items.filter((d: Department) => d.isActive),
      error: (err: any) => console.error(err)
    });

    this.orgService.getDesignations().subscribe({
      next: (res: any) => this.designations = res.items.filter((d: Designation) => d.isActive),
      error: (err: any) => console.error(err)
    });
  }

  loadEmployeeDetails(id: number): void {
    this.isLoading = true;
    this.employeeService.getEmployeeById(id).subscribe({
      next: (emp: Employee) => {
        this.employeeForm.patchValue({
          employeeCode: emp.employeeCode,
          firstName: emp.firstName,
          lastName: emp.lastName,
          email: emp.email,
          phoneNumber: emp.phoneNumber || '',
          dateOfBirth: emp.dateOfBirth ? emp.dateOfBirth.substring(0, 10) : '',
          gender: emp.gender || 'Male',
          address: emp.address || '',
          departmentId: emp.departmentId,
          designationId: emp.designationId,
          salary: emp.salary,
          dateOfJoining: emp.dateOfJoining ? emp.dateOfJoining.substring(0, 10) : '',
          employmentType: emp.employmentType || 'Full-Time',
          profileImageUrl: emp.profileImageUrl || '',
          isActive: emp.isActive
        });
        this.isLoading = false;
      },
      error: (err: any) => {
        this.errorMessage = 'Unable to fetch employee details.';
        this.isLoading = false;
        console.error(err);
      }
    });
  }

  onSubmit(): void {
    if (this.employeeForm.invalid) {
      this.employeeForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;
    this.errorMessage = '';
    const formValues = this.employeeForm.value;

    if (this.isEditMode && this.employeeId) {
      this.employeeService.updateEmployee(this.employeeId, formValues).subscribe({
        next: () => {
          this.isSaving = false;
          this.router.navigate(['/employees'], {
            queryParams: { msg: 'Employee updated successfully.' }
          });
        },
        error: (err: any) => {
          this.isSaving = false;
          this.errorMessage = err.error?.message || 'Error updating employee record.';
        }
      });
    } else {
      this.employeeService.createEmployee(formValues).subscribe({
        next: () => {
          this.isSaving = false;
          this.router.navigate(['/employees'], {
            queryParams: { msg: 'Employee registered successfully.' }
          });
        },
        error: (err: any) => {
          this.isSaving = false;
          this.errorMessage = err.error?.message || 'Error creating employee record.';
        }
      });
    }
  }

  onCancel(): void {
    this.router.navigate(['/employees']);
  }
}
