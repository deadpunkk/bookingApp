namespace BookingApi.Dto;

public record CreateBookingDto(string Title, DateTime StartDate, DateTime EndDate);