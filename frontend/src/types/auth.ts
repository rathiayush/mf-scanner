export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  user: User;
  expiresAt: string;
}

export interface User {
  id: number;
  username: string;
  email: string;
  role: 'Admin' | 'ServiceUser';
  isActive: boolean;
  createdDate: string;
  lastLoginDate?: string;
  createdByUsername?: string;
  notes?: string;
}

export interface CreateUserRequest {
  username: string;
  email: string;
  password: string;
  role: string;
  notes?: string;
}

export interface UpdateUserRequest {
  email?: string;
  role?: string;
  isActive?: boolean;
  notes?: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

export interface ResetPasswordRequest {
  userId: number;
  newPassword: string;
}

export interface UsersListResponse {
  users: User[];
  totalCount: number;
}

export interface AuthContextType {
  user: User | null;
  token: string | null;
  login: (username: string, password: string) => Promise<void>;
  logout: () => void;
  isAuthenticated: boolean;
  isAdmin: boolean;
  loading: boolean;
}
