import { createContext, useContext, useState, type ReactNode } from 'react';
import { apiClient } from '../api/client';
import type { AuthResponse } from '../types';

interface AuthContextType {
  userId: string | null;
  isAuthenticated: boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (fullName: string, email: string, password: string) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [userId, setUserId] = useState<string | null>(
    localStorage.getItem('userId')
  );

  const saveAuth = (data: AuthResponse, uid: string) => {
    localStorage.setItem('accessToken', data.accessToken);
    localStorage.setItem('refreshToken', data.refreshToken);
    localStorage.setItem('userId', uid);
    setUserId(uid);
  };

  const login = async (email: string, password: string) => {
    const res = await apiClient.post<AuthResponse>('/api/auth/login', { email, password });
    const payload = JSON.parse(atob(res.data.accessToken.split('.')[1]));
    saveAuth(res.data, payload.sub);
  };

  const register = async (fullName: string, email: string, password: string) => {
    const res = await apiClient.post<AuthResponse>('/api/auth/register', {
      fullName, email, password,
    });
    const payload = JSON.parse(atob(res.data.accessToken.split('.')[1]));
    saveAuth(res.data, payload.sub);
  };

  const logout = () => {
    localStorage.clear();
    setUserId(null);
  };

  return (
    <AuthContext.Provider value={{
      userId,
      isAuthenticated: !!userId,
      login, register, logout,
    }}>
      {children}
    </AuthContext.Provider>
  );
}

export const useAuth = () => {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used inside AuthProvider');
  return ctx;
};
