import { BrowserRouter, Navigate, Route, Routes } from 'react-router';

import { Layout } from '../shared/components/Layout';
import { NotFoundPage } from '../shared/components/NotFoundPage';
import { ProtectedRoute } from '../shared/components/ProtectedRoute';

import { LoginPage } from '../pages/LoginPage/LoginPage';
import { BookingsPage } from '../pages/BookingsPage/BookingsPage';
import { MyBookingsPage } from '../pages/MyBookingsPage/MyBookingsPage';
import { CreateBookingPage } from '../pages/CreateBookingPage/CreateBookingPage';
import { EditBookingPage } from '../pages/EditBookingPage/EditBookingPage';

export function AppRouter() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Layout />}>
          <Route index element={<Navigate to="/bookings" replace />} />
          <Route path="login" element={<LoginPage />} />

          <Route element={<ProtectedRoute />}>
            <Route path="bookings" element={<BookingsPage />} />
            <Route path="my-bookings" element={<MyBookingsPage />} />
          </Route>

          <Route element={<ProtectedRoute allowedRoles={['Admin', 'Moderator']} />}>
            <Route path="bookings/create" element={<CreateBookingPage />} />
            <Route path="bookings/:id/edit" element={<EditBookingPage />} />
          </Route>

          <Route path="*" element={<NotFoundPage />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}
