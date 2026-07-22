import type { ReactNode } from 'react';

import type { UserRole } from '../../features/auth/authTypes';
import { useAuth } from '../../features/auth/useAuth';

type RoleGuardProps = {
  allowedRoles: readonly UserRole[];
  children: ReactNode;
  fallback?: ReactNode;
};

export function RoleGuard({ allowedRoles, children, fallback = null }: RoleGuardProps) {
  const { user } = useAuth();

  if (!user || !allowedRoles.includes(user.role)) {
    return fallback;
  }

  return children;
}
