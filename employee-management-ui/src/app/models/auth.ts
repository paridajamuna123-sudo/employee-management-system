export interface LoginRequest {
  email: string;
  password: string;
  rememberMe?: boolean;
}

export interface LoginResponse {
  userId: string;
  email: string;
  fullName: string;
  roles: string[];
  token: string;
  expiration: string;
}

export interface UserProfile {
  userId: string;
  email: string;
  fullName: string;
  roles: string[];
  employeeId?: number;
}

export interface ChartItem {
  name: string;
  count: number;
}

export interface DashboardSummary {
  totalEmployees: number;
  activeEmployees: number;
  inactiveEmployees: number;
  newJoinersThisMonth: number;
  totalMonthlyPayroll: number;
  averageSalary: number;
  departmentDistribution: ChartItem[];
  designationDistribution: ChartItem[];
}

export interface RecentEmployee {
  id: number;
  employeeCode: string;
  firstName: string;
  lastName: string;
  email: string;
  department: string;
  designation: string;
  salary: number;
  dateOfJoining: string;
  isActive: boolean;
}
