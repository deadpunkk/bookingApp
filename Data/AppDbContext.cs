using Microsoft.EntityFrameworkCore;
using BookingApi.Domain;

namespace BookingApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<User> Users => Set<User>();
}