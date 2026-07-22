import { Navigate, Outlet, useLocation } from 'react-router';

import type { UserRole } from '../../features/auth/authTypes';
import { useAuth } from '../../features/auth/useAuth';

type ProtectedRouteProps = {
  allowedRoles?: readonly UserRole[];
};

export function ProtectedRoute({ allowedRoles }: ProtectedRouteProps) {
  const { isAuthenticated, user } = useAuth();
  const location = useLocation();

  if (!isAuthenticated || !user) {
    return (
      <Navigate
        to="/login"
        replace
        state={{
          from: {
            pathname: location.pathname,
            search: location.search,
            hash: location.hash,
          },
        }}
      />
    );
  }

  if (allowedRoles && !allowedRoles.includes(user.role)) {
    return <Navigate to="/bookings" replace />;
  }

  return <Outlet />;
}
