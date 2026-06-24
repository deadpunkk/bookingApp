import { useParams } from 'react-router';

export function EditBookingPage() {
  const { id } = useParams();

  return (
    <section className="page">
      <h1>Редактирование брони</h1>
      <p>Здесь позже будет форма редактирования бронирования.</p>
      <p>ID бронирования: {id}</p>
    </section>
  );
}