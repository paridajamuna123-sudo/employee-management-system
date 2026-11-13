import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  Department,
  DepartmentRequest,
  Designation,
  DesignationRequest,
  Employee,
  EmployeeQueryParams,
  EmployeeRequest,
  OrgQueryParams,
  PagedResult
} from '../models/employee';

@Injectable({
  providedIn: 'root'
})
export class OrgService {
  private apiUrl = 'https://localhost:7043/api';

  constructor(private http: HttpClient) {}

  // Departments
  getDepartments(params?: OrgQueryParams): Observable<PagedResult<Department>> {
    let httpParams = new HttpParams();
    if (params) {
      httpParams = httpParams
        .set('pageNumber', params.pageNumber.toString())
        .set('pageSize', params.pageSize.toString());
      if (params.searchTerm) httpParams = httpParams.set('searchTerm', params.searchTerm);
      if (params.isActive !== undefined) httpParams = httpParams.set('isActive', params.isActive.toString());
    } else {
      httpParams = httpParams.set('pageNumber', '1').set('pageSize', '100');
    }
    return this.http.get<PagedResult<Department>>(`${this.apiUrl}/departments`, { params: httpParams });
  }

  getDepartmentById(id: number): Observable<Department> {
    return this.http.get<Department>(`${this.apiUrl}/departments/${id}`);
  }

  createDepartment(data: DepartmentRequest): Observable<Department> {
    return this.http.post<Department>(`${this.apiUrl}/departments`, data);
  }

  updateDepartment(id: number, data: DepartmentRequest): Observable<Department> {
    return this.http.put<Department>(`${this.apiUrl}/departments/${id}`, data);
  }

  deleteDepartment(id: number): Observable<unknown> {
    return this.http.delete(`${this.apiUrl}/departments/${id}`);
  }

  // Designations
  getDesignations(params?: OrgQueryParams): Observable<PagedResult<Designation>> {
    let httpParams = new HttpParams();
    if (params) {
      httpParams = httpParams
        .set('pageNumber', params.pageNumber.toString())
        .set('pageSize', params.pageSize.toString());
      if (params.searchTerm) httpParams = httpParams.set('searchTerm', params.searchTerm);
      if (params.isActive !== undefined) httpParams = httpParams.set('isActive', params.isActive.toString());
    } else {
      httpParams = httpParams.set('pageNumber', '1').set('pageSize', '100');
    }
    return this.http.get<PagedResult<Designation>>(`${this.apiUrl}/designations`, { params: httpParams });
  }

  getDesignationById(id: number): Observable<Designation> {
    return this.http.get<Designation>(`${this.apiUrl}/designations/${id}`);
  }

  createDesignation(data: DesignationRequest): Observable<Designation> {
    return this.http.post<Designation>(`${this.apiUrl}/designations`, data);
  }

  updateDesignation(id: number, data: DesignationRequest): Observable<Designation> {
    return this.http.put<Designation>(`${this.apiUrl}/designations/${id}`, data);
  }

  deleteDesignation(id: number): Observable<unknown> {
    return this.http.delete(`${this.apiUrl}/designations/${id}`);
  }
}
