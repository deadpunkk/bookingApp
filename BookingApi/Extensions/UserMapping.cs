using BookingApi.Domain;
using BookingApi.Dto;

namespace BookingApi.Extensions;

public static class UserMapping
{
    public static UserDto ToDto(this User user)
    {
        return new UserDto(
            user.Id,
            user.Login,
            user.Role
        );
    }

    public static IReadOnlyList<UserDto> ToDtoList(this IReadOnlyList<User> users)
    {
        return users.Select(b => b.ToDto()).ToList();
    }
}