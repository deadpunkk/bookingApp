export type Booking = {
  id: number;
  title: string;
  startDate: string;
  endDate: string;
};

export type PagedResult<TItem> = {
  items: TItem[];
  page: number;
  pageSize: number;
  totalCount: number;
};

export type BookingRequest = {
  title: string;
  startDate: string;
  endDate: string;
};

export type CreateBookingRequest = BookingRequest;
export type UpdateBookingRequest = BookingRequest;
