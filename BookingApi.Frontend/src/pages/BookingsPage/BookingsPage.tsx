import { useEffect, useState } from 'react';

import { BookingTable } from '../../features/bookings/BookingTable';
import type { Booking, PagedResult } from '../../features/bookings/bookingTypes';
import { deleteBooking, getBookings } from '../../features/bookings/bookingsApi';
import { useAuth } from '../../features/auth/useAuth';
import { getApiErrorMessage } from '../../shared/api/getApiErrorMessage';

const PAGE_SIZE = 10;

export function BookingsPage() {
  const { token, isAuthenticated } = useAuth();

  const [bookingsResult, setBookingsResult] = useState<PagedResult<Booking> | null>(null);
  const [page, setPage] = useState(1);
  const [refreshKey, setRefreshKey] = useState(0);
  const [isLoading, setIsLoading] = useState(true);
  const [deletingId, setDeletingId] = useState<number | null>(null);
  const [error, setError] = useState<string | null>(null);

  const totalPages = bookingsResult
    ? Math.max(1, Math.ceil(bookingsResult.totalCount / (bookingsResult.pageSize || PAGE_SIZE)))
    : 1;

  useEffect(() => {
    const accessToken = token;
    if (!isAuthenticated || !accessToken) {
      return;
    }

    const controller = new AbortController();

    async function loadBookings(validToken: string) {
      setIsLoading(true);
      setError(null);

      try {
        const result = await getBookings({
          accessToken: validToken,
          page,
          pageSize: PAGE_SIZE,
          signal: controller.signal,
        });

        setBookingsResult(result);
      } catch (requestError) {
        if (!controller.signal.aborted) {
          setError(getApiErrorMessage(requestError, 'Не удалось загрузить бронирования.'));
        }
      } finally {
        if (!controller.signal.aborted) {
          setIsLoading(false);
        }
      }
    }

    void loadBookings(accessToken);

    return () => controller.abort();
  }, [isAuthenticated, token, page, refreshKey]);

  async function handleDelete(booking: Booking) {
    if (!token || !window.confirm(`Удалить бронь «${booking.title}»?`)) {
      return;
    }

    setDeletingId(booking.id);
    setError(null);

    try {
      await deleteBooking(booking.id, token);

      if (bookingsResult?.items.length === 1 && page > 1) {
        setPage((currentPage) => currentPage - 1);
      } else {
        setRefreshKey((current) => current + 1);
      }
    } catch (requestError) {
      setError(getApiErrorMessage(requestError, 'Не удалось удалить бронирование.'));
    } finally {
      setDeletingId(null);
    }
  }

  return (
    <section className="page">
      <div className="page-header">
        <div>
          <h1>Все бронирования</h1>
          <p>Список всех бронирований с серверной пагинацией.</p>
        </div>
      </div>

      {isLoading && <div className="status-message">Загрузка бронирований...</div>}

      {error && (
        <div className="form-error" role="alert">
          {error}
        </div>
      )}

      {!isLoading && bookingsResult && (
        <>
          {bookingsResult.items.length > 0 ? (
            <BookingTable
              bookings={bookingsResult.items}
              deletingId={deletingId}
              onDelete={(booking) => void handleDelete(booking)}
            />
          ) : (
            <div className="empty-state">Бронирований пока нет.</div>
          )}

          <div className="pagination" aria-label="Пагинация">
            <span className="pagination-total">
              Всего: {bookingsResult.totalCount}
            </span>
            <button
              type="button"
              disabled={page === 1}
              onClick={() => setPage((currentPage) => currentPage - 1)}
            >
              Назад
            </button>
            <span>
              Страница {bookingsResult.page} из {totalPages}
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
