import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { DashboardSummary, RecentEmployee } from '../models/auth';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private apiUrl = 'https://localhost:7043/api/dashboard';

  constructor(private http: HttpClient) {}

  getSummary(): Observable<DashboardSummary> {
    return this.http.get<DashboardSummary>(`${this.apiUrl}/summary`);
  }

  getRecentEmployees(count: number = 5): Observable<RecentEmployee[]> {
    return this.http.get<RecentEmployee[]>(`${this.apiUrl}/recent-employees?count=${count}`);
  }
}
