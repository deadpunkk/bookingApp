using BookingApi.Domain;

namespace BookingApi.Services;

public interface IJwtService
{
    string GenerateToken(User user);
}