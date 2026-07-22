using BookingApi.Common;

namespace BookingApi.Dto;

public record CreateUserDto(string Login, string Password, Roles Role);