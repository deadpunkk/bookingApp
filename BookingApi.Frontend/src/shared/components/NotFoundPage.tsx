import { Link } from 'react-router';

export function NotFoundPage() {
  return (
    <section className="page">
      <h1>Страница не найдена</h1>
      <p>Такого маршрута в приложении нет.</p>
      <Link to="/bookings">Вернуться к бронированиям</Link>
    </section>
  );
}