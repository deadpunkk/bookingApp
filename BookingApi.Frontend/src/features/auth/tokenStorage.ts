import type {AuthUser} from './authTypes';

const TOKEN_KEY = 'bookingApi.auth.token';
const USER_KEY = 'bookingApi.auth.user';

export function getStoredToken(): string | null{
    return localStorage.getItem(TOKEN_KEY);
}

export function getStoredUser(): AuthUser | null{
    const rawUser = localStorage.getItem(USER_KEY);

    if (!rawUser){
        return null;
    }

    try {
        return JSON.parse(rawUser) as AuthUser;
    } catch {
        localStorage.removeItem(USER_KEY);
        return null;
    }
}

export function saveAuth(token: string, user: AuthUser): void {
        localStorage.setItem(TOKEN_KEY, token);
        localStorage.setItem(USER_KEY, JSON.stringify(user));
}

export function clearAuthStorage(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
}