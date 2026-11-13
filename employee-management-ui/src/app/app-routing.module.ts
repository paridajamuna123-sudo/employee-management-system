import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { LoginComponent } from './components/login/login.component';
import { MainLayoutComponent } from './components/layout/main-layout.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { EmployeeListComponent } from './components/employee-list/employee-list.component';
import { EmployeeFormComponent } from './components/employees/employee-form/employee-form.component';
import { EmployeeProfileComponent } from './components/employees/employee-profile/employee-profile.component';
import { DepartmentListComponent } from './components/departments/department-list.component';
import { DesignationListComponent } from './components/designations/designation-list.component';
import { AttendanceComponent } from './components/attendance/attendance.component';
import { LeavesComponent } from './components/leaves/leaves.component';
import { SettingsComponent } from './components/settings/settings.component';
import { AuthGuard, RoleGuard } from './guards/auth.guard';

const routes: Routes = [
  { path: 'login', component: LoginComponent },
  {
    path: '',
    component: MainLayoutComponent,
    canActivate: [AuthGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: DashboardComponent },
      {
        path: 'employees',
        component: EmployeeListComponent,
        canActivate: [RoleGuard],
        data: { roles: ['Admin', 'HR', 'Manager'] }
      },
      {
        path: 'employees/add',
        component: EmployeeFormComponent,
        canActivate: [RoleGuard],
        data: { roles: ['Admin', 'HR'] }
      },
      {
        path: 'employees/edit/:id',
        component: EmployeeFormComponent,
        canActivate: [RoleGuard],
        data: { roles: ['Admin', 'HR'] }
      },
      {
        path: 'employees/profile/:id',
        component: EmployeeProfileComponent
      },
      {
        path: 'departments',
        component: DepartmentListComponent,
        canActivate: [RoleGuard],
        data: { roles: ['Admin', 'HR'] }
      },
      {
        path: 'designations',
        component: DesignationListComponent,
        canActivate: [RoleGuard],
        data: { roles: ['Admin', 'HR'] }
      },
      {
        path: 'attendance',
        component: AttendanceComponent
      },
      {
        path: 'leaves',
        component: LeavesComponent
      },
      { path: 'settings', component: SettingsComponent }
    ]
  },
  { path: '**', redirectTo: '' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
