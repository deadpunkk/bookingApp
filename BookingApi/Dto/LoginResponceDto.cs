namespace BookingApi.Dto;

public record LoginResponseDto(
    string AccessToken,
    UserDto User
);