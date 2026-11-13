import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { EmployeeService } from '../../services/employee.service';
import { OrgService } from '../../services/org.service';
import { Department, Designation, Employee, EmployeeQueryParams, PagedResult } from '../../models/employee';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-employee-list',
  templateUrl: './employee-list.component.html',
  styleUrls: ['./employee-list.component.scss']
})
export class EmployeeListComponent implements OnInit {
  employees: Employee[] = [];
  departments: Department[] = [];
  designations: Designation[] = [];
  employeeToDelete: Employee | null = null;

  isLoading = false;
  isExporting = false;
  successMessage = '';
  errorMessage = '';

  queryParams: EmployeeQueryParams = {
    pageNumber: 1,
    pageSize: 10,
    searchTerm: '',
    departmentId: undefined,
    designationId: undefined,
    employmentType: '',
    isActive: undefined,
    sortBy: 'Id',
    sortOrder: 'desc'
  };

  pagedResult: PagedResult<Employee> = {
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
    private employeeService: EmployeeService,
    private orgService: OrgService,
    public authService: AuthService,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      if (params['msg']) {
        this.successMessage = params['msg'];
      }
    });

    this.loadFilterData();
    this.loadEmployees();
  }

  loadFilterData(): void {
    this.orgService.getDepartments().subscribe({
      next: (res) => this.departments = res.items,
      error: (err) => console.error(err)
    });

    this.orgService.getDesignations().subscribe({
      next: (res) => this.designations = res.items,
      error: (err) => console.error(err)
    });
  }

  loadEmployees(): void {
    this.isLoading = true;
    this.employeeService.getEmployees(this.queryParams).subscribe({
      next: (result) => {
        this.pagedResult = result;
        this.employees = result.items;
        this.isLoading = false;
      },
      error: (error) => {
        this.errorMessage = 'Unable to load employee directory.';
        this.isLoading = false;
        console.error(error);
      }
    });
  }

  onSearch(event: Event): void {
    const val = (event.target as HTMLInputElement).value;
    clearTimeout(this.searchDebounce);
    this.searchDebounce = setTimeout(() => {
      this.queryParams.searchTerm = val;
      this.queryParams.pageNumber = 1;
      this.loadEmployees();
    }, 350);
  }

  onDepartmentChange(event: Event): void {
    const val = (event.target as HTMLSelectElement).value;
    this.queryParams.departmentId = val ? +val : undefined;
    this.queryParams.pageNumber = 1;
    this.loadEmployees();
  }

  onDesignationChange(event: Event): void {
    const val = (event.target as HTMLSelectElement).value;
    this.queryParams.designationId = val ? +val : undefined;
    this.queryParams.pageNumber = 1;
    this.loadEmployees();
  }

  onEmploymentTypeChange(event: Event): void {
    const val = (event.target as HTMLSelectElement).value;
    this.queryParams.employmentType = val;
    this.queryParams.pageNumber = 1;
    this.loadEmployees();
  }

  onStatusChange(status: 'all' | 'active' | 'inactive'): void {
    if (status === 'all') this.queryParams.isActive = undefined;
    else if (status === 'active') this.queryParams.isActive = true;
    else this.queryParams.isActive = false;

    this.queryParams.pageNumber = 1;
    this.loadEmployees();
  }

  resetFilters(): void {
    this.queryParams = {
      pageNumber: 1,
      pageSize: 10,
      searchTerm: '',
      departmentId: undefined,
      designationId: undefined,
      employmentType: '',
      isActive: undefined,
      sortBy: 'Id',
      sortOrder: 'desc'
    };
    this.loadEmployees();
  }

  onSort(columnName: string): void {
    if (this.queryParams.sortBy === columnName) {
      this.queryParams.sortOrder = this.queryParams.sortOrder === 'asc' ? 'desc' : 'asc';
    } else {
      this.queryParams.sortBy = columnName;
      this.queryParams.sortOrder = 'asc';
    }
    this.queryParams.pageNumber = 1;
    this.loadEmployees();
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.pagedResult.totalPages || page === this.queryParams.pageNumber) return;
    this.queryParams.pageNumber = page;
    this.loadEmployees();
  }

  onPageSizeChange(event: Event): void {
    this.queryParams.pageSize = +(event.target as HTMLSelectElement).value;
    this.queryParams.pageNumber = 1;
    this.loadEmployees();
  }

  toggleStatus(emp: Employee): void {
    const targetStatus = !emp.isActive;
    this.employeeService.updateStatus(emp.id, targetStatus).subscribe({
      next: () => {
        emp.isActive = targetStatus;
        this.successMessage = `Employee ${emp.firstName} ${emp.lastName} status changed to ${targetStatus ? 'Active' : 'Inactive'}.`;
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Failed to update employee status.';
      }
    });
  }

  promptDelete(emp: Employee): void {
    this.employeeToDelete = emp;
  }

  cancelDelete(): void {
    this.employeeToDelete = null;
  }

  confirmDelete(): void {
    if (!this.employeeToDelete) return;

    this.employeeService.deleteEmployee(this.employeeToDelete.id).subscribe({
      next: () => {
        this.successMessage = `Employee record for ${this.employeeToDelete?.firstName} ${this.employeeToDelete?.lastName} deleted.`;
        this.employeeToDelete = null;
        this.loadEmployees();
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Error deleting employee.';
        this.employeeToDelete = null;
      }
    });
  }

  exportToExcel(): void {
    this.isExporting = true;
    this.employeeService.exportExcel(this.queryParams).subscribe({
      next: (blob) => {
        this.isExporting = false;
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `Employees_${new Date().toISOString().substring(0, 10)}.xlsx`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
      },
      error: (err) => {
        this.isExporting = false;
        this.errorMessage = 'Failed to generate Excel export.';
        console.error(err);
      }
    });
  }
}
