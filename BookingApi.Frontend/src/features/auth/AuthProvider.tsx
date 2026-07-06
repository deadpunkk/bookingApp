import { useState, type ReactNode } from 'react';

import { login as loginRequest } from './authApi';
import { AuthContext } from './AuthContext';
import type { AuthUser, LoginRequest } from './authTypes';
import {
  clearAuthStorage,
  getStoredToken,
  getStoredUser,
  saveAuth,
} from './tokenStorage';

type AuthProviderProps = {
  children: ReactNode;
};

export function AuthProvider({ children }: AuthProviderProps) {
  const [token, setToken] = useState<string | null>(() => getStoredToken());
  const [user, setUser] = useState<AuthUser | null>(() => getStoredUser());

  const isAuthenticated = token !== null && user !== null;

  async function login(request: LoginRequest): Promise<void> {
    const result = await loginRequest(request);

    setToken(result.token);
    setUser(result.user);

    saveAuth(result.token, result.user);
  }

  function logout(): void {
    setToken(null);
    setUser(null);

    clearAuthStorage();
  }

  const value = {
    token,
    user,
    isAuthenticated,
    login,
    logout,
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
}