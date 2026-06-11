using Microsoft.EntityFrameworkCore;
using BookingApi.Common;
using BookingApi.Domain;

namespace BookingApi.Data;

public static class DbSeeder
{
    public static async Task SeedAdminAsync(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var adminExists =
            await db.Users.AnyAsync(
                u => u.Login == "admin");

        if (adminExists)
        {
            return;
        }
        var login = configuration["AdminSettings:Login"];
        var password = configuration["AdminSettings:Password"];
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        
        if (string.IsNullOrWhiteSpace(login) ||
            string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "Admin credentials are not configured");
        }
        
        var admin = new User(login, passwordHash, Roles.Admin);
        
        await db.Users.AddAsync(admin);
        await db.SaveChangesAsync();
    }
}