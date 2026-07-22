import { Link } from 'react-router';

import { RoleGuard } from '../../shared/components/RoleGuard';

import type { Booking } from './bookingTypes';

type BookingTableProps = {
  bookings: Booking[];
  deletingId: number | null;
  onDelete: (booking: Booking) => void;
};

export function BookingTable({ bookings, deletingId, onDelete }: BookingTableProps) {
  return (
    <div className="table-wrapper">
      <table className="table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Название</th>
            <th>Начало</th>
            <th>Конец</th>
            <th>Действия</th>
          </tr>
        </thead>
        <tbody>
          {bookings.map((booking) => (
            <tr key={booking.id}>
              <td>{booking.id}</td>
              <td>{booking.title}</td>
              <td>{formatDateTime(booking.startDate)}</td>
              <td>{formatDateTime(booking.endDate)}</td>
              <td>
                <RoleGuard
                  allowedRoles={['Admin', 'Moderator']}
                  fallback={<span className="muted">Только просмотр</span>}
                >
                  <div className="table-actions">
                    <Link className="button button-secondary" to={`/bookings/${booking.id}/edit`}>
                      Редактировать
                    </Link>
                    <button
                      className="button button-danger"
                      type="button"
                      disabled={deletingId !== null}
                      onClick={() => onDelete(booking)}
                    >
                      {deletingId === booking.id ? 'Удаляем...' : 'Удалить'}
                    </button>
                  </div>
                </RoleGuard>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

function formatDateTime(value: string): string {
  const date = new Date(value);

  if (Number.isNaN(date.getTime())) {
    return value;
  }

  return new Intl.DateTimeFormat('ru-RU', {
    dateStyle: 'short',
    timeStyle: 'short',
  }).format(date);
}
