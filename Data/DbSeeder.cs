using Microsoft.EntityFrameworkCore;
using BookingApi.Common;
using BookingApi.Domain;

namespace BookingApi.Data;

public static class DbSeeder
{
    public static async Task SeedAdminAsync(IServiceProvider serviceProvider)
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

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("admin12345");
        var admin = new User("admin", passwordHash, Roles.Admin);
        
        await db.Users.AddAsync(admin);
        await db.SaveChangesAsync();
    }
}