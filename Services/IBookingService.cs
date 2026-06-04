using BookingApi.Domain;
using BookingApi.Dto;
using BookingApi.Common;


namespace BookingApi.Services;

public interface IBookingService
{
    Task<Result<IReadOnlyList<Booking>>> GetAllAsync();
    Task<Result<Booking>> GetByIdAsync(int id);
    Task<Result<Booking>> CreateAsync(CreateBookingDto dto);
    Task<Result<Booking>> UpdateAsync(int id, UpdateBookingDto dto);
    Task<Result<bool>> DeleteAsync(int id);
}