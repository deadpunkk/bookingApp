import { useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router';

import { useAuth } from '../../features/auth/useAuth';
import {
  BookingForm,
  type BookingFormValues,
} from '../../features/bookings/BookingForm';
import type { Booking } from '../../features/bookings/bookingTypes';
import { getBookingById, updateBooking } from '../../features/bookings/bookingsApi';
import { getApiErrorMessage } from '../../shared/api/getApiErrorMessage';

export function EditBookingPage() {
  const { id } = useParams();
  const { token } = useAuth();
  const navigate = useNavigate();
  const bookingId = Number(id);
  const hasValidId = Number.isInteger(bookingId) && bookingId > 0;

  const [booking, setBooking] = useState<Booking | null>(null);
  const [isLoading, setIsLoading] = useState(hasValidId);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const accessToken = token;
    if (!hasValidId || !accessToken) {
      return;
    }

    const controller = new AbortController();

    async function loadBooking(validToken: string) {
      setIsLoading(true);
      setError(null);

      try {
        setBooking(await getBookingById(bookingId, validToken, controller.signal));
      } catch (requestError) {
        if (!controller.signal.aborted) {
          setError(getApiErrorMessage(requestError, 'Не удалось загрузить бронирование.'));
        }
      } finally {
        if (!controller.signal.aborted) {
          setIsLoading(false);
        }
      }
    }

    void loadBooking(accessToken);

    return () => controller.abort();
  }, [token, bookingId, hasValidId]);

  async function handleSubmit(values: BookingFormValues) {
    if (!token || !hasValidId) {
      return;
    }

    setIsSubmitting(true);
    setError(null);

    try {
      await updateBooking(bookingId, values, token);
      navigate('/bookings', { replace: true });
    } catch (requestError) {
      setError(getApiErrorMessage(requestError, 'Не удалось сохранить бронирование.'));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <section className="page">
      <div className="page-header">
        <div>
          <h1>Редактирование брони</h1>
          <p>Изменение бронирования #{hasValidId ? bookingId : id}.</p>
        </div>
      </div>

      {!hasValidId && (
        <div className="form-error" role="alert">
          Некорректный ID бронирования.
        </div>
      )}

      {isLoading && <div className="status-message">Загрузка бронирования...</div>}

      {!isLoading && error && booking?.id !== bookingId && (
        <>
          <div className="form-error" role="alert">{error}</div>
          <Link className="back-link" to="/bookings">Вернуться к списку</Link>
        </>
      )}

      {!isLoading && hasValidId && booking?.id === bookingId && (
        <BookingForm
          initialValues={{
            title: booking.title,
            startDate: toDateTimeLocalValue(booking.startDate),
            endDate: toDateTimeLocalValue(booking.endDate),
          }}
          submitLabel="Сохранить изменения"
          isSubmitting={isSubmitting}
          error={error}
          onSubmit={handleSubmit}
        />
      )}
    </section>
  );
}

function toDateTimeLocalValue(value: string): string {
  const date = new Date(value);

  if (Number.isNaN(date.getTime())) {
    return value.slice(0, 16);
  }

  const pad = (part: number) => String(part).padStart(2, '0');

  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`
    + `T${pad(date.getHours())}:${pad(date.getMinutes())}`;
}
