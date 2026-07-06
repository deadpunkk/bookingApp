import {createContext} from 'react';

import type {AuthUser, LoginRequest} from './authTypes';

export type AuthContextValue = {
    token: string | null,
    user: AuthUser | null,
    isAuthenticated: boolean,
    login: (request: LoginRequest) => Promise<void>,
    logout: () => void
}

export const AuthContext = createContext<AuthContextValue | null>(null);