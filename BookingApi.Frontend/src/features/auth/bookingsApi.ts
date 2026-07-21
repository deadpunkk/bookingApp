import { ApiClient } from '../../shared/api/apiClient';

import type {
  Booking,
  CreateBookingRequest,
  PagedResult,
  UpdateBookingRequest,
} from './bookingTypes';

type AuthorizedRequest = {
  accessToken: string;
};

type GetBookingsParams = AuthorizedRequest & {
  page: number;
  pageSize: number;
};

export function getBookings(params: GetBookingsParams): Promise<PagedResult<Booking>> {
  const searchParams = new URLSearchParams({
    page: String(params.page),
    pageSize: String(params.pageSize),
  });

  return ApiClient<PagedResult<Booking>>(`/bookings?${searchParams.toString()}`, {
    accessToken: params.accessToken,
  });
}

export function getMyBookings(accessToken: string): Promise<Booking[]> {
  return ApiClient<Booking[]>('/bookings/my', {
    accessToken,
  });
}

export function getBookingById(id: number, accessToken: string): Promise<Booking> {
  return ApiClient<Booking>(`/bookings/${id}`, {
    accessToken,
  });
}

export function createBooking(
  request: CreateBookingRequest,
  accessToken: string,
): Promise<Booking> {
  return ApiClient<Booking>('/bookings', {
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
  return ApiClient<void>(`/bookings/${id}`, {
    method: 'PUT',
    accessToken,
    body: JSON.stringify(request),
  });
}

export function deleteBooking(id: number, accessToken: string): Promise<void> {
  return ApiClient<void>(`/bookings/${id}`, {
    method: 'DELETE',
    accessToken,
  });
}