import { useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router';

import { useAuth } from '../../features/auth/useAuth';

export function LoginPage() {
  const [loginValue, setLoginValue] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  const { login: signIn } = useAuth();
  const navigate = useNavigate();

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    setError(null);
    setIsLoading(true);

    try {
      await signIn({
        login: loginValue,
        password,
      });

      navigate('/bookings', { replace: true });
    } catch (error) {
      const message = error instanceof Error
        ? error.message
        : 'Не удалось выполнить вход';

      setError(message);
    } finally {
      setIsLoading(false);
    }
  }

  return (
    <section className="page">
      <h1>Вход</h1>

      <form className="form" onSubmit={handleSubmit}>
        <label className="form-field">
          <span>Логин</span>
          <input
            type="text"
            value={loginValue}
            onChange={(event) => setLoginValue(event.target.value)}
            autoComplete="username"
          />
        </label>

        <label className="form-field">
          <span>Пароль</span>
          <input
            type="password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            autoComplete="current-password"
          />
        </label>

        {error && (
          <div className="form-error">
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