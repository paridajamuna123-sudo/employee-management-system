import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { OrgService } from '../../services/org.service';
import { Designation, OrgQueryParams, PagedResult } from '../../models/employee';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-designation-list',
  templateUrl: './designation-list.component.html',
  styleUrls: ['./designation-list.component.scss']
})
export class DesignationListComponent implements OnInit {
  designations: Designation[] = [];
  desigForm: FormGroup;
  editingDesigId: number | null = null;
  showModal = false;
  desigToDelete: Designation | null = null;

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

  pagedResult: PagedResult<Designation> = {
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
    this.desigForm = this.fb.group({
      designationName: ['', [Validators.required, Validators.maxLength(100)]],
      description: ['', [Validators.maxLength(500)]],
      isActive: [true]
    });
  }

  ngOnInit(): void {
    this.loadDesignations();
  }

  loadDesignations(): void {
    this.isLoading = true;
    this.orgService.getDesignations(this.queryParams).subscribe({
      next: (res: PagedResult<Designation>) => {
        this.pagedResult = res;
        this.designations = res.items;
        this.isLoading = false;
      },
      error: (err: any) => {
        this.errorMessage = 'Failed to load designations.';
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
      this.loadDesignations();
    }, 350);
  }

  openCreateModal(): void {
    this.editingDesigId = null;
    this.desigForm.reset({
      designationName: '',
      description: '',
      isActive: true
    });
    this.showModal = true;
    this.errorMessage = '';
  }

  openEditModal(desig: Designation): void {
    this.editingDesigId = desig.id;
    this.desigForm.patchValue({
      designationName: desig.designationName,
      description: desig.description,
      isActive: desig.isActive
    });
    this.showModal = true;
    this.errorMessage = '';
  }

  closeModal(): void {
    this.showModal = false;
    this.editingDesigId = null;
  }

  saveDesignation(): void {
    if (this.desigForm.invalid) {
      this.desigForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;
    this.errorMessage = '';

    if (this.editingDesigId) {
      this.orgService.updateDesignation(this.editingDesigId, this.desigForm.value).subscribe({
        next: () => {
          this.isSaving = false;
          this.successMessage = 'Designation updated successfully.';
          this.closeModal();
          this.loadDesignations();
        },
        error: (err: any) => {
          this.isSaving = false;
          this.errorMessage = err.error?.message || 'Error updating designation.';
        }
      });
    } else {
      this.orgService.createDesignation(this.desigForm.value).subscribe({
        next: () => {
          this.isSaving = false;
          this.successMessage = 'Designation created successfully.';
          this.closeModal();
          this.loadDesignations();
        },
        error: (err: any) => {
          this.isSaving = false;
          this.errorMessage = err.error?.message || 'Error creating designation.';
        }
      });
    }
  }

  promptDelete(desig: Designation): void {
    this.desigToDelete = desig;
  }

  cancelDelete(): void {
    this.desigToDelete = null;
  }

  confirmDelete(): void {
    if (!this.desigToDelete) return;

    this.orgService.deleteDesignation(this.desigToDelete.id).subscribe({
      next: () => {
        this.successMessage = `Designation '${this.desigToDelete?.designationName}' deleted successfully.`;
        this.desigToDelete = null;
        this.loadDesignations();
      },
      error: (err: any) => {
        this.errorMessage = err.error?.message || 'Failed to delete designation.';
        this.desigToDelete = null;
        console.error(err);
      }
    });
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.pagedResult.totalPages || page === this.queryParams.pageNumber) return;
    this.queryParams.pageNumber = page;
    this.loadDesignations();
  }
}
