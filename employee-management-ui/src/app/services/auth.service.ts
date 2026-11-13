import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { LoginRequest, LoginResponse, UserProfile } from '../models/auth';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'https://localhost:7043/api/auth';
  private currentUserSubject = new BehaviorSubject<LoginResponse | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient, private router: Router) {
    const saved = localStorage.getItem('ems_user') || sessionStorage.getItem('ems_user');
    if (saved) {
      try {
        const user = JSON.parse(saved);
        if (new Date(user.expiration) > new Date()) {
          this.currentUserSubject.next(user);
        } else {
          this.clearSession();
        }
      } catch {
        this.clearSession();
      }
    }
  }

  public get currentUserValue(): LoginResponse | null {
    return this.currentUserSubject.value;
  }

  public get token(): string | null {
    return this.currentUserValue ? this.currentUserValue.token : null;
  }

  public isAuthenticated(): boolean {
    const user = this.currentUserValue;
    if (!user || !user.token) return false;
    return new Date(user.expiration) > new Date();
  }

  public hasRole(role: string): boolean {
    return !!this.currentUserValue?.roles?.includes(role);
  }

  public hasAnyRole(roles: string[]): boolean {
    if (!this.currentUserValue || !this.currentUserValue.roles) return false;
    return roles.some(r => this.currentUserValue?.roles.includes(r));
  }

  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, credentials).pipe(
      tap((res) => {
        if (credentials.rememberMe) {
          localStorage.setItem('ems_user', JSON.stringify(res));
        } else {
          sessionStorage.setItem('ems_user', JSON.stringify(res));
        }
        this.currentUserSubject.next(res);
      })
    );
  }

  logout(): void {
    if (this.token) {
      this.http.post(`${this.apiUrl}/logout`, {}).subscribe({
        error: () => {}
      });
    }
    this.clearSession();
    this.router.navigate(['/login']);
  }

  private clearSession(): void {
    localStorage.removeItem('ems_user');
    sessionStorage.removeItem('ems_user');
    this.currentUserSubject.next(null);
  }
}
