import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Employee, EmployeeQueryParams, EmployeeRequest, PagedResult } from '../models/employee';

@Injectable({
  providedIn: 'root'
})
export class EmployeeService {
  private apiUrl = 'https://localhost:7043/api/employees';

  constructor(private http: HttpClient) {}

  getEmployees(params: EmployeeQueryParams): Observable<PagedResult<Employee>> {
    let httpParams = new HttpParams()
      .set('pageNumber', params.pageNumber.toString())
      .set('pageSize', params.pageSize.toString());

    if (params.searchTerm && params.searchTerm.trim()) {
      httpParams = httpParams.set('searchTerm', params.searchTerm.trim());
    }

    if (params.departmentId) {
      httpParams = httpParams.set('departmentId', params.departmentId.toString());
    }

    if (params.designationId) {
      httpParams = httpParams.set('designationId', params.designationId.toString());
    }

    if (params.employmentType) {
      httpParams = httpParams.set('employmentType', params.employmentType);
    }

    if (params.isActive !== undefined && params.isActive !== null) {
      httpParams = httpParams.set('isActive', params.isActive.toString());
    }

    if (params.sortBy) {
      httpParams = httpParams.set('sortBy', params.sortBy);
    }

    if (params.sortOrder) {
      httpParams = httpParams.set('sortOrder', params.sortOrder);
    }

    return this.http.get<PagedResult<Employee>>(this.apiUrl, { params: httpParams });
  }

  getEmployeeById(id: number): Observable<Employee> {
    return this.http.get<Employee>(`${this.apiUrl}/${id}`);
  }

  createEmployee(data: EmployeeRequest): Observable<any> {
    return this.http.post<any>(this.apiUrl, data);
  }

  updateEmployee(id: number, data: EmployeeRequest): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/${id}`, data);
  }

  updateStatus(id: number, isActive: boolean): Observable<any> {
    return this.http.patch<any>(`${this.apiUrl}/${id}/status`, { isActive });
  }

  linkAccount(id: number, userId: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/${id}/link-account`, { userId });
  }

  deleteEmployee(id: number): Observable<unknown> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  exportExcel(params: EmployeeQueryParams): Observable<Blob> {
    let httpParams = new HttpParams();
    if (params.searchTerm) httpParams = httpParams.set('searchTerm', params.searchTerm.trim());
    if (params.departmentId) httpParams = httpParams.set('departmentId', params.departmentId.toString());
    if (params.designationId) httpParams = httpParams.set('designationId', params.designationId.toString());
    if (params.isActive !== undefined && params.isActive !== null) httpParams = httpParams.set('isActive', params.isActive.toString());

    return this.http.get(`${this.apiUrl}/export`, {
      params: httpParams,
      responseType: 'blob'
    });
  }
}
