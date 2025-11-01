import axios from 'axios';
import {
  LoginRequest,
  LoginResponse,
  User,
  CreateUserRequest,
  UpdateUserRequest,
  ChangePasswordRequest,
  ResetPasswordRequest,
  UsersListResponse,
} from '../types/auth';
import env from '../config/env';

const API_BASE_URL = env.api.baseUrl;

// Create axios instance with interceptor for auth token
const api = axios.create({
  baseURL: API_BASE_URL,
});

// Add token to requests
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Handle 401 responses
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Token expired or invalid
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      localStorage.removeItem('tokenExpiry');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export const authService = {
  // Authentication endpoints
  login: async (request: LoginRequest): Promise<LoginResponse> => {
    const response = await api.post(`/auth/login`, request);
    return response.data;
  },

  getCurrentUser: async (): Promise<User> => {
    const response = await api.get('/auth/me');
    return response.data;
  },

  changePassword: async (request: ChangePasswordRequest): Promise<void> => {
    await api.post('/auth/change-password', request);
  },

  // User management endpoints (Admin only)
  getAllUsers: async (includeInactive = false): Promise<UsersListResponse> => {
    const response = await api.get('/usermanagement', {
      params: { includeInactive },
    });
    return response.data;
  },

  getUserById: async (userId: number): Promise<User> => {
    const response = await api.get(`/usermanagement/${userId}`);
    return response.data;
  },

  createUser: async (request: CreateUserRequest): Promise<User> => {
    const response = await api.post('/usermanagement', request);
    return response.data;
  },

  updateUser: async (userId: number, request: UpdateUserRequest): Promise<User> => {
    const response = await api.put(`/usermanagement/${userId}`, request);
    return response.data;
  },

  deleteUser: async (userId: number): Promise<void> => {
    await api.delete(`/usermanagement/${userId}`);
  },

  activateUser: async (userId: number): Promise<void> => {
    await api.post(`/usermanagement/${userId}/activate`);
  },

  deactivateUser: async (userId: number): Promise<void> => {
    await api.post(`/usermanagement/${userId}/deactivate`);
  },

  resetPassword: async (request: ResetPasswordRequest): Promise<void> => {
    await api.post('/usermanagement/reset-password', request);
  },
};
