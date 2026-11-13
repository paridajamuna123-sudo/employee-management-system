import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { LoginResponse } from '../../models/auth';

@Component({
  selector: 'app-main-layout',
  templateUrl: './main-layout.component.html',
  styleUrls: ['./main-layout.component.scss']
})
export class MainLayoutComponent implements OnInit {
  isSidebarCollapsed = false;
  isMobileOpen = false;
  currentUser: LoginResponse | null = null;

  constructor(private authService: AuthService) {}

  ngOnInit(): void {
    this.currentUser = this.authService.currentUserValue;
    this.authService.currentUser$.subscribe(u => this.currentUser = u);
  }

  toggleSidebar(): void {
    this.isSidebarCollapsed = !this.isSidebarCollapsed;
  }

  toggleMobileSidebar(): void {
    this.isMobileOpen = !this.isMobileOpen;
  }

  closeMobileSidebar(): void {
    this.isMobileOpen = false;
  }

  canAccessEmployees(): boolean {
    return this.authService.hasAnyRole(['Admin', 'HR', 'Manager']);
  }

  canAccessOrg(): boolean {
    return this.authService.hasAnyRole(['Admin', 'HR']);
  }

  logout(): void {
    this.authService.logout();
  }
}
