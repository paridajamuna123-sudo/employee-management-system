import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { OrgService } from '../../services/org.service';
import { Department, OrgQueryParams, PagedResult } from '../../models/employee';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-department-list',
  templateUrl: './department-list.component.html',
  styleUrls: ['./department-list.component.scss']
})
export class DepartmentListComponent implements OnInit {
  departments: Department[] = [];
  deptForm: FormGroup;
  editingDeptId: number | null = null;
  showModal = false;
  deptToDelete: Department | null = null;

  isLoading = false;
  isSaving = false;
  successMessage = '';
  errorMessage = '';

  queryParams: OrgQueryParams = {
    pageNumber: 1,
    pageSize: 10,
    searchTerm: '',
    isActive: undefined
  };

  pagedResult: PagedResult<Department> = {
    items: [],
    totalCount: 0,
    pageNumber: 1,
    pageSize: 10,
    totalPages: 0,
    hasPreviousPage: false,
    hasNextPage: false
  };

  private searchDebounce: any;

  constructor(
    private fb: FormBuilder,
    private orgService: OrgService,
    public authService: AuthService
  ) {
    this.deptForm = this.fb.group({
      departmentName: ['', [Validators.required, Validators.maxLength(100)]],
      description: ['', [Validators.maxLength(500)]],
      isActive: [true]
    });
  }

  ngOnInit(): void {
    this.loadDepartments();
  }

  loadDepartments(): void {
    this.isLoading = true;
    this.orgService.getDepartments(this.queryParams).subscribe({
      next: (res: PagedResult<Department>) => {
        this.pagedResult = res;
        this.departments = res.items;
        this.isLoading = false;
      },
      error: (err: any) => {
        this.errorMessage = 'Failed to load departments.';
        this.isLoading = false;
        console.error(err);
      }
    });
  }

  onSearch(event: Event): void {
    const val = (event.target as HTMLInputElement).value;
    clearTimeout(this.searchDebounce);
    this.searchDebounce = setTimeout(() => {
      this.queryParams.searchTerm = val;
      this.queryParams.pageNumber = 1;
      this.loadDepartments();
    }, 350);
  }

  openCreateModal(): void {
    this.editingDeptId = null;
    this.deptForm.reset({
      departmentName: '',
      description: '',
      isActive: true
    });
    this.showModal = true;
    this.errorMessage = '';
  }

  openEditModal(dept: Department): void {
    this.editingDeptId = dept.id;
    this.deptForm.patchValue({
      departmentName: dept.departmentName,
      description: dept.description,
      isActive: dept.isActive
    });
    this.showModal = true;
    this.errorMessage = '';
  }

  closeModal(): void {
    this.showModal = false;
    this.editingDeptId = null;
  }

  saveDepartment(): void {
    if (this.deptForm.invalid) {
      this.deptForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;
    this.errorMessage = '';

    if (this.editingDeptId) {
      this.orgService.updateDepartment(this.editingDeptId, this.deptForm.value).subscribe({
        next: () => {
          this.isSaving = false;
          this.successMessage = 'Department updated successfully.';
          this.closeModal();
          this.loadDepartments();
        },
        error: (err: any) => {
          this.isSaving = false;
          this.errorMessage = err.error?.message || 'Error updating department.';
        }
      });
    } else {
      this.orgService.createDepartment(this.deptForm.value).subscribe({
        next: () => {
          this.isSaving = false;
          this.successMessage = 'Department created successfully.';
          this.closeModal();
          this.loadDepartments();
        },
        error: (err: any) => {
          this.isSaving = false;
          this.errorMessage = err.error?.message || 'Error creating department.';
        }
      });
    }
  }

  promptDelete(dept: Department): void {
    this.deptToDelete = dept;
  }

  cancelDelete(): void {
    this.deptToDelete = null;
  }

  confirmDelete(): void {
    if (!this.deptToDelete) return;

    this.orgService.deleteDepartment(this.deptToDelete.id).subscribe({
      next: () => {
        this.successMessage = `Department '${this.deptToDelete?.departmentName}' deleted successfully.`;
        this.deptToDelete = null;
        this.loadDepartments();
      },
      error: (err: any) => {
        this.errorMessage = err.error?.message || 'Failed to delete department.';
        this.deptToDelete = null;
        console.error(err);
      }
    });
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.pagedResult.totalPages || page === this.queryParams.pageNumber) return;
    this.queryParams.pageNumber = page;
    this.loadDepartments();
  }
}
