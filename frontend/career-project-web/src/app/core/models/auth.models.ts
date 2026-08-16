export type UserRole = 'Person' | 'Company';

export interface RegisterPersonRequest {
  email: string;
  password: string;
  fullName: string;
}

export interface RegisterCompanyRequest {
  email: string;
  password: string;
  companyName: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  userId: string;
  email: string;
  role: UserRole;
  displayName: string;
}
