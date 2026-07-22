import type { ApiUserRole, AuthUser } from './authTypes';
import { normalizeRole } from './roleMapper';

const TOKEN_KEY = 'bookingApi.auth.token';
const USER_KEY = 'bookingApi.auth.user';

export function getStoredToken(): string | null {
  const token = localStorage.getItem(TOKEN_KEY);

  if (!token?.trim()) {
    localStorage.removeItem(TOKEN_KEY);
    return null;
  }

  return token.trim();
}

export function getStoredUser(): AuthUser | null {
  const rawUser = localStorage.getItem(USER_KEY);

  if (!rawUser) {
    return null;
  }

  try {
    const user = JSON.parse(rawUser) as Partial<AuthUser> & { role?: ApiUserRole };

    if (typeof user.id !== 'number' || typeof user.login !== 'string' || user.role === undefined) {
      throw new Error('Invalid stored user');
    }

    return {
      id: user.id,
      login: user.login,
      role: normalizeRole(user.role),
    };
  } catch {
    localStorage.removeItem(USER_KEY);
    return null;
  }
}

export function saveAuth(token: string, user: AuthUser): void {
  if (!token.trim()) {
    throw new Error('Backend did not return access token');
  }

  localStorage.setItem(TOKEN_KEY, token);
  localStorage.setItem(USER_KEY, JSON.stringify(user));
}

export function clearAuthStorage(): void {
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem(USER_KEY);
}
