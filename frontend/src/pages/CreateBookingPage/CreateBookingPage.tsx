import { useState } from 'react';
import { useNavigate } from 'react-router';

import { useAuth } from '../../features/auth/useAuth';
import {
  BookingForm,
  type BookingFormValues,
} from '../../features/bookings/BookingForm';
import { createBooking } from '../../features/bookings/bookingsApi';
import { getApiErrorMessage } from '../../shared/api/getApiErrorMessage';

const initialValues: BookingFormValues = {
  title: '',
  startDate: '',
  endDate: '',
};

export function CreateBookingPage() {
  const { token } = useAuth();
  const navigate = useNavigate();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleSubmit(values: BookingFormValues) {
    if (!token) {
      setError('Сессия истекла. Войдите снова.');
      return;
    }

    setIsSubmitting(true);
    setError(null);

    try {
      await createBooking(values, token);
      navigate('/bookings', { replace: true });
    } catch (requestError) {
      setError(getApiErrorMessage(requestError, 'Не удалось создать бронирование.'));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <section className="page">
      <div className="page-header">
        <div>
          <h1>Создание брони</h1>
          <p>Укажите название и свободный временной интервал.</p>
        </div>
      </div>

      <BookingForm
        initialValues={initialValues}
        submitLabel="Создать бронь"
        isSubmitting={isSubmitting}
        error={error}
        onSubmit={handleSubmit}
      />
    </section>
  );
}
