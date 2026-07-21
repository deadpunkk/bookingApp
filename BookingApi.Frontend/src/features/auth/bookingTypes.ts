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

export type CreateBookingRequest = {
  title: string;
  startDate: string;
  endDate: string;
};

export type UpdateBookingRequest = {
  title: string;
  startDate: string;
  endDate: string;
};