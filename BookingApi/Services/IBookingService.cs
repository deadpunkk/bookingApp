using BookingApi.Domain;
using BookingApi.Dto;
using BookingApi.Common;


namespace BookingApi.Services;

public interface IBookingService
{
    Task<Result<PagedResult<Booking>>> GetAllAsync(int page, int pageSize);
    Task<Result<Booking>> GetByIdAsync(int id);
    Task<Result<Booking>> CreateAsync(CreateBookingDto dto, int userId);
    Task<Result<Booking>> UpdateAsync(int id, UpdateBookingDto dto, int userId, string userRole);
    Task<Result<IReadOnlyList<Booking>>> GetMyAsync(int userId);
    Task<Result<bool>> DeleteAsync(int id, int userId, string userRole);
}