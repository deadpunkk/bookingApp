import { useState, type FormEvent } from 'react';

export type BookingFormValues = {
  title: string;
  startDate: string;
  endDate: string;
};

type BookingFormProps = {
  initialValues: BookingFormValues;
  submitLabel: string;
  isSubmitting: boolean;
  error: string | null;
  onSubmit: (values: BookingFormValues) => Promise<void> | void;
};

export function BookingForm({
  initialValues,
  submitLabel,
  isSubmitting,
  error,
  onSubmit,
}: BookingFormProps) {
  const [values, setValues] = useState(initialValues);
  const [validationError, setValidationError] = useState<string | null>(null);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    const message = validateBooking(values);
    if (message) {
      setValidationError(message);
      return;
    }

    setValidationError(null);
    await onSubmit({
      ...values,
      title: values.title.trim(),
    });
  }

  return (
    <form className="form booking-form" onSubmit={handleSubmit} noValidate>
      <label className="form-field">
        <span>Название</span>
        <input
          type="text"
          value={values.title}
          onChange={(event) => setValues((current) => ({
            ...current,
            title: event.target.value,
          }))}
          disabled={isSubmitting}
          autoFocus
        />
      </label>

      <label className="form-field">
        <span>Начало</span>
        <input
          type="datetime-local"
          value={values.startDate}
          onChange={(event) => setValues((current) => ({
            ...current,
            startDate: event.target.value,
          }))}
          disabled={isSubmitting}
        />
      </label>

      <label className="form-field">
        <span>Конец</span>
        <input
          type="datetime-local"
          value={values.endDate}
          onChange={(event) => setValues((current) => ({
            ...current,
            endDate: event.target.value,
          }))}
          disabled={isSubmitting}
        />
      </label>

      {(validationError || error) && (
        <div className="form-error" role="alert">
          {validationError || error}
        </div>
      )}

      <button type="submit" disabled={isSubmitting}>
        {isSubmitting ? 'Сохраняем...' : submitLabel}
      </button>
    </form>
  );
}

function validateBooking(values: BookingFormValues): string | null {
  if (!values.title.trim()) {
    return 'Введите название бронирования.';
  }

  if (!values.startDate) {
    return 'Укажите дату и время начала.';
  }

  if (!values.endDate) {
    return 'Укажите дату и время окончания.';
  }

  const startTime = new Date(values.startDate).getTime();
  const endTime = new Date(values.endDate).getTime();

  if (Number.isNaN(startTime) || Number.isNaN(endTime)) {
    return 'Укажите корректные дату и время.';
  }

  if (endTime <= startTime) {
    return 'Дата окончания должна быть позже даты начала.';
  }

  return null;
}
