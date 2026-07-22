import { Link, NavLink, Outlet } from 'react-router';

import { useAuth } from '../../features/auth/useAuth';
import { RoleGuard } from './RoleGuard';

export function Layout() {
  const { user, isAuthenticated, logout } = useAuth();

  return (
    <div className="app">
      <header className="app-header">
        <Link className="app-logo" to={isAuthenticated ? '/bookings' : '/login'}>
          BookingApi
        </Link>

        {isAuthenticated && (
          <nav className="app-nav" aria-label="Основная навигация">
            <NavLink to="/bookings" end>Все бронирования</NavLink>
            <NavLink to="/my-bookings">Мои бронирования</NavLink>
            <RoleGuard allowedRoles={['Admin', 'Moderator']}>
              <NavLink to="/bookings/create">Создать бронь</NavLink>
            </RoleGuard>
          </nav>
        )}

        <div className="app-auth">
          {isAuthenticated && user ? (
            <>
              <span className="app-user" title="Текущий пользователь и роль">
                {user.login} · {user.role}
              </span>

              <button className="link-button" type="button" onClick={logout}>
                Выйти
              </button>
            </>
          ) : (
            <Link to="/login">Войти</Link>
          )}
        </div>
      </header>

      <main className="app-main">
        <Outlet />
      </main>
    </div>
  );
}
