import api from './api';

interface LoginCredentials {
  username: string;
  password: string;
}

interface RegisterCredentials {
  username: string;
  password: string;
  role?: string;
}

interface TokenPayload {
  unique_name?: string;
  name?: string;
  role?: string;
  exp?: number;
}

const parseJwtPayload = (token: string): TokenPayload | null => {
  try {
    const payload = token.split('.')[1];
    const base64 = payload.replace(/-/g, '+').replace(/_/g, '/');
    const decoded = atob(base64);
    const jsonPayload = decodeURIComponent(
      decoded
        .split('')
        .map((c) => `%${('00' + c.charCodeAt(0).toString(16)).slice(-2)}`)
        .join('')
    );
    return JSON.parse(jsonPayload);
  } catch {
    return null;
  }
};

export const authService = {
  login: async (credentials: LoginCredentials) => {
    const response = await api.post<{ token?: string; Token?: string }>('/auth/login', credentials);
    const token = response.data.token || response.data.Token;
    if (token) {
      localStorage.setItem('token', token);
    }
    return response.data;
  },
  register: async (credentials: RegisterCredentials) => {
    const response = await api.post('/auth/register', credentials);
    return response.data;
  },
  getToken: () => localStorage.getItem('token'),
  getCurrentUser: () => {
    const token = localStorage.getItem('token');
    if (!token) return null;
    const payload = parseJwtPayload(token);
    if (!payload) {
      localStorage.removeItem('token');
      return null;
    }
    if (payload.exp && payload.exp * 1000 < Date.now()) {
      localStorage.removeItem('token');
      return null;
    }
    return {
      username: payload.unique_name || payload.name || '',
      role: payload.role || 'User',
    };
  },
  getUserRole: () => authService.getCurrentUser()?.role,
  isAuthenticated: () => !!authService.getCurrentUser(),
  isAdmin: () => authService.getCurrentUser()?.role === 'Admin',
  logout: () => {
    localStorage.removeItem('token');
  },
};
