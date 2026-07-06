import type { ApiUser, AuthUser, UserRole } from './authTypes';



export function normalizeRole(role: ApiUser['role']): UserRole {
  if (role === 'Admin' || role === 0) {
    return 'Admin';
  }

  if (role === 'Moderator' || role === 1) {
    return 'Moderator';
  }

  if (role === 'Viewer' || role === 2) {
    return 'Viewer';
  }

  throw new Error(`Unknown user role: ${role}`);
}

export function mapApiUserToAuthUser(user: ApiUser): AuthUser{
    return{
        id: user.id,
        login: user.login,
        role: normalizeRole(user.role)
    };
}

