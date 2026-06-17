using BookingApi.Domain;
using BookingApi.Dto;

namespace BookingApi.Extensions;

public static class BookingMapping
{
    public static BookingDto ToDto(this Booking booking)
    {
        return new  BookingDto(
            booking.Id,
            booking.Title,
            booking.StartDate,
            booking.EndDate
        );
    }

    public static IReadOnlyList<BookingDto> ToDtoList(this IReadOnlyList<Booking> bookings)
    {
        return bookings.Select(b => b.ToDto()).ToList();
    }
}