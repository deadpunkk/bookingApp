namespace BookingApi.Dto;

public record BookingDto(
    int Id,
    string Title,
    DateTime StartDate,
    DateTime EndDate
);