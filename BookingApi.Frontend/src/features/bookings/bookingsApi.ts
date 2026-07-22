import { apiClient } from '../../shared/api/apiClient';

import type {
  Booking,
  CreateBookingRequest,
  PagedResult,
  UpdateBookingRequest,
} from './bookingTypes';

type GetBookingsParams = {
  accessToken: string;
  page: number;
  pageSize: number;
  signal?: AbortSignal;
};

export function getBookings(params: GetBookingsParams): Promise<PagedResult<Booking>> {
  const searchParams = new URLSearchParams({
    page: String(params.page),
    pageSize: String(params.pageSize),
  });

  return apiClient<PagedResult<Booking>>(`/bookings?${searchParams.toString()}`, {
    accessToken: params.accessToken,
    signal: params.signal,
  });
}

export function getMyBookings(accessToken: string, signal?: AbortSignal): Promise<Booking[]> {
  return apiClient<Booking[]>('/bookings/my', { accessToken, signal });
}

export function getBookingById(
  id: number,
  accessToken: string,
  signal?: AbortSignal,
): Promise<Booking> {
  return apiClient<Booking>(`/bookings/${id}`, { accessToken, signal });
}

export function createBooking(
  request: CreateBookingRequest,
  accessToken: string,
): Promise<Booking> {
  return apiClient<Booking>('/bookings', {
    method: 'POST',
    accessToken,
    body: JSON.stringify(request),
  });
}

export function updateBooking(
  id: number,
  request: UpdateBookingRequest,
  accessToken: string,
): Promise<void> {
  return apiClient<void>(`/bookings/${id}`, {
    method: 'PUT',
    accessToken,
    body: JSON.stringify(request),
  });
}

export function deleteBooking(id: number, accessToken: string): Promise<void> {
  return apiClient<void>(`/bookings/${id}`, {
    method: 'DELETE',
    accessToken,
  });
}
