using BookingApi.Common;

namespace BookingApi.Dto;

public record UserDto(int Id, string Login, Roles Role);