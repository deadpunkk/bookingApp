import { useEffect, useState } from 'react';

import { useAuth } from '../../features/auth/useAuth';
import { BookingTable } from '../../features/bookings/BookingTable';
import type { Booking } from '../../features/bookings/bookingTypes';
import { deleteBooking, getMyBookings } from '../../features/bookings/bookingsApi';
import { getApiErrorMessage } from '../../shared/api/getApiErrorMessage';

export function MyBookingsPage() {
  const { token } = useAuth();
  const [bookings, setBookings] = useState<Booking[]>([]);
  const [refreshKey, setRefreshKey] = useState(0);
  const [isLoading, setIsLoading] = useState(true);
  const [deletingId, setDeletingId] = useState<number | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const accessToken = token;
    if (!accessToken) {
      return;
    }

    const controller = new AbortController();

    async function loadBookings(validToken: string) {
      setIsLoading(true);
      setError(null);

      try {
        setBookings(await getMyBookings(validToken, controller.signal));
      } catch (requestError) {
        if (!controller.signal.aborted) {
          setError(getApiErrorMessage(requestError, 'Не удалось загрузить ваши бронирования.'));
        }
      } finally {
        if (!controller.signal.aborted) {
          setIsLoading(false);
        }
      }
    }

    void loadBookings(accessToken);

    return () => controller.abort();
  }, [token, refreshKey]);

  async function handleDelete(booking: Booking) {
    if (!token || !window.confirm(`Удалить бронь «${booking.title}»?`)) {
      return;
    }

    setDeletingId(booking.id);
    setError(null);

    try {
      await deleteBooking(booking.id, token);
      setRefreshKey((current) => current + 1);
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
          <h1>Мои бронирования</h1>
          <p>Бронирования, созданные текущим пользователем.</p>
        </div>
      </div>

      {isLoading && <div className="status-message">Загрузка бронирований...</div>}

      {error && (
        <div className="form-error" role="alert">
          {error}
        </div>
      )}

      {!isLoading && bookings.length > 0 && (
        <BookingTable
          bookings={bookings}
          deletingId={deletingId}
          onDelete={(booking) => void handleDelete(booking)}
        />
      )}

      {!isLoading && bookings.length === 0 && !error && (
        <div className="empty-state">У вас пока нет бронирований.</div>
      )}
    </section>
  );
}
