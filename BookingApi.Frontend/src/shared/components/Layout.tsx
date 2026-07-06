import { Link, Outlet } from 'react-router';

import { useAuth } from '../../features/auth/useAuth';

export function Layout() {
  const { user, isAuthenticated, logout } = useAuth();

  return (
    <div className="app">
      <header className="app-header">
        <div className="app-logo">BookingApi</div>

        <nav className="app-nav">
          <Link to="/bookings">Все бронирования</Link>
          <Link to="/my-bookings">Мои бронирования</Link>
          <Link to="/bookings/create">Создать бронь</Link>
        </nav>

        <div className="app-auth">
          {isAuthenticated && user ? (
            <>
              <span className="app-user">
                {user.login} / {user.role}
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