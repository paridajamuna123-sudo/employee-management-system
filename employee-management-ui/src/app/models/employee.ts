export interface Department {
  id: number;
  departmentName: string;
  description?: string;
  isActive: boolean;
  employeeCount: number;
  createdAt: string;
  updatedAt?: string;
}

export interface DepartmentRequest {
  departmentName: string;
  description?: string;
  isActive: boolean;
}

export interface Designation {
  id: number;
  designationName: string;
  description?: string;
  isActive: boolean;
  employeeCount: number;
  createdAt: string;
  updatedAt?: string;
}

export interface DesignationRequest {
  designationName: string;
  description?: string;
  isActive: boolean;
}

export interface OrgQueryParams {
  pageNumber: number;
  pageSize: number;
  searchTerm?: string;
  isActive?: boolean;
}

export interface Employee {
  id: number;
  employeeCode: string;
  firstName: string;
  lastName: string;
  fullName?: string;
  email: string;
  phoneNumber?: string;
  dateOfBirth?: string;
  gender?: string;
  address?: string;
  departmentId?: number;
  departmentName?: string;
  designationId?: number;
  designationName?: string;
  salary: number;
  dateOfJoining: string;
  employmentType: string;
  profileImageUrl?: string;
  isActive: boolean;
  createdAt?: string;
  updatedAt?: string;
  isAccountLinked?: boolean;
  linkedUserId?: string;
  linkedUserEmail?: string;
}

export interface EmployeeRequest {
  employeeCode?: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber?: string;
  dateOfBirth?: string;
  gender?: string;
  address?: string;
  departmentId?: number;
  designationId?: number;
  salary: number;
  dateOfJoining: string;
  employmentType: string;
  profileImageUrl?: string;
  isActive: boolean;
}

export interface EmployeeQueryParams {
  pageNumber: number;
  pageSize: number;
  searchTerm?: string;
  departmentId?: number;
  designationId?: number;
  employmentType?: string;
  isActive?: boolean;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}
