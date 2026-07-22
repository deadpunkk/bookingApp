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

type AuthState = {
  token: string | null;
  user: AuthUser | null;
};

export function AuthProvider({ children }: AuthProviderProps) {
  const [auth, setAuth] = useState<AuthState>(() => {
    const token = getStoredToken();
    const user = getStoredUser();

    if (!token || !user) {
      clearAuthStorage();
      return { token: null, user: null };
    }

    return { token, user };
  });

  const isAuthenticated = auth.token !== null && auth.user !== null;

  async function login(request: LoginRequest): Promise<void> {
    const result = await loginRequest(request);

    saveAuth(result.token, result.user);
    setAuth(result);
  }

  function logout(): void {
    clearAuthStorage();
    setAuth({ token: null, user: null });
  }

  const value = {
    token: auth.token,
    user: auth.user,
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
