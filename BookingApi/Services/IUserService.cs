using BookingApi.Domain;
using BookingApi.Common;
using BookingApi.Dto;

namespace BookingApi.Services;

public interface IUserService
{
    Task<Result<IReadOnlyList<User>>> GetAllAsync();
    Task<Result<User>> GetByIdAsync(int id);
    Task<Result<User>> CreateAsync(CreateUserDto dto);
    Task<Result<User>> LoginAsync(LoginDto dto);

}