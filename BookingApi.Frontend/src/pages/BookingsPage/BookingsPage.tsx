import { useEffect, useState } from 'react';

import { ApiError } from '../../shared/api/apiClient';
import { getBookings } from '../../features/auth/bookingsApi';
import type { Booking, PagedResult } from '../../features/auth/bookingTypes';
import { useAuth } from '../../features/auth/useAuth';

const PAGE_SIZE = 10;

export function BookingsPage() {
  const { token, isAuthenticated } = useAuth();

  const [bookingsResult, setBookingsResult] = useState<PagedResult<Booking> | null>(null);
  const [page, setPage] = useState(1);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const totalPages = bookingsResult
    ? Math.max(1, Math.ceil(bookingsResult.totalCount / bookingsResult.pageSize))
    : 1;

  useEffect(() => {
    if (!isAuthenticated || !token) {
      setBookingsResult(null);
      setError('Для просмотра бронирований нужно войти.');
      return;
    }

    async function loadBookings() {
      setIsLoading(true);
      setError(null);

      try {
        const result = await getBookings({
          accessToken: token,
          page,
          pageSize: PAGE_SIZE,
        });

        setBookingsResult(result);
      } catch (error) {
        const message = error instanceof ApiError
          ? getApiErrorMessage(error)
          : 'Не удалось загрузить бронирования';

        setError(message);
      } finally {
        setIsLoading(false);
      }
    }

    void loadBookings();
  }, [isAuthenticated, token, page]);

  return (
    <section className="page">
      <div className="page-header">
        <div>
          <h1>Все бронирования</h1>
          <p>Список всех бронирований с серверной пагинацией.</p>
        </div>
      </div>

      {isLoading && <p>Загрузка...</p>}

      {error && (
        <div className="form-error">
          {error}
        </div>
      )}

      {!isLoading && !error && bookingsResult && (
        <>
          <table className="table">
            <thead>
              <tr>
                <th>ID</th>
                <th>Название</th>
                <th>Начало</th>
                <th>Конец</th>
              </tr>
            </thead>

            <tbody>
              {bookingsResult.items.map((booking) => (
                <tr key={booking.id}>
                  <td>{booking.id}</td>
                  <td>{booking.title}</td>
                  <td>{formatDateTime(booking.startDate)}</td>
                  <td>{formatDateTime(booking.endDate)}</td>
                </tr>
              ))}
            </tbody>
          </table>

          {bookingsResult.items.length === 0 && (
            <p>Бронирований пока нет.</p>
          )}

          <div className="pagination">
            <button
              type="button"
              disabled={page === 1}
              onClick={() => setPage((currentPage) => currentPage - 1)}
            >
              Назад
            </button>

            <span>
              Страница {page} из {totalPages}
            </span>

            <button
              type="button"
              disabled={page >= totalPages}
              onClick={() => setPage((currentPage) => currentPage + 1)}
            >
              Вперёд
            </button>
          </div>
        </>
      )}
    </section>
  );
}

function formatDateTime(value: string): string {
  return new Intl.DateTimeFormat('ru-RU', {
    dateStyle: 'short',
    timeStyle: 'short',
  }).format(new Date(value));
}

function getApiErrorMessage(error: ApiError): string {
  if (error.status === 401) {
    return 'Сессия истекла или токен недействителен. Войдите снова.';
  }

  if (error.status === 403) {
    return 'Недостаточно прав для выполнения действия.';
  }

  return error.message;
}