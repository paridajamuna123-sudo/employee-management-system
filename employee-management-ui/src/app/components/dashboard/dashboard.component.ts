import { Component, OnInit } from '@angular/core';
import { DashboardService } from '../../services/dashboard.service';
import { DashboardSummary, RecentEmployee } from '../../models/auth';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  summary: DashboardSummary = {
    totalEmployees: 0,
    activeEmployees: 0,
    inactiveEmployees: 0,
    newJoinersThisMonth: 0,
    totalMonthlyPayroll: 0,
    averageSalary: 0,
    departmentDistribution: [],
    designationDistribution: []
  };
  recentEmployees: RecentEmployee[] = [];
  isLoading = true;
  errorMessage = '';

  constructor(private dashboardService: DashboardService) {}

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.dashboardService.getSummary().subscribe({
      next: (data) => {
        this.summary = data;
        this.loadRecentEmployees();
      },
      error: (err) => {
        this.errorMessage = 'Unable to fetch dashboard statistics.';
        this.isLoading = false;
        console.error(err);
      }
    });
  }

  loadRecentEmployees(): void {
    this.dashboardService.getRecentEmployees(5).subscribe({
      next: (data) => {
        this.recentEmployees = data;
        this.isLoading = false;
      },
      error: (err) => {
        this.isLoading = false;
        console.error(err);
      }
    });
  }
}
