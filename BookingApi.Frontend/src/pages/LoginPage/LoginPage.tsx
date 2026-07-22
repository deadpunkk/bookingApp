import { useState, type FormEvent } from 'react';
import { Navigate, useLocation, useNavigate } from 'react-router';

import { useAuth } from '../../features/auth/useAuth';
import { getApiErrorMessage } from '../../shared/api/getApiErrorMessage';

type LoginLocationState = {
  from?: {
    pathname: string;
    search: string;
    hash: string;
  };
};

export function LoginPage() {
  const [loginValue, setLoginValue] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  const { isAuthenticated, login: signIn } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const redirectLocation = (location.state as LoginLocationState | null)?.from;
  const redirectTo = redirectLocation
    ? `${redirectLocation.pathname}${redirectLocation.search}${redirectLocation.hash}`
    : '/bookings';

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    setError(null);
    setIsLoading(true);

    try {
      await signIn({
        login: loginValue,
        password,
      });

      navigate(redirectTo, { replace: true });
    } catch (requestError) {
      setError(getApiErrorMessage(requestError, 'Не удалось выполнить вход.'));
    } finally {
      setIsLoading(false);
    }
  }

  if (isAuthenticated) {
    return <Navigate to="/bookings" replace />;
  }

  return (
    <section className="page login-page">
      <div className="page-header">
        <div>
          <h1>Вход</h1>
          <p>Войдите, чтобы управлять бронированиями.</p>
        </div>
      </div>

      <form className="form" onSubmit={handleSubmit}>
        <label className="form-field">
          <span>Логин</span>
          <input
            type="text"
            value={loginValue}
            onChange={(event) => setLoginValue(event.target.value)}
            autoComplete="username"
            required
            autoFocus
          />
        </label>

        <label className="form-field">
          <span>Пароль</span>
          <input
            type="password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            autoComplete="current-password"
            required
          />
        </label>

        {error && (
          <div className="form-error" role="alert">
            {error}
          </div>
        )}

        <button type="submit" disabled={isLoading}>
          {isLoading ? 'Входим...' : 'Войти'}
        </button>
      </form>
    </section>
  );
}
