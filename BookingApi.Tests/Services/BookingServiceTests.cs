using Xunit;
using BookingApi.Data;
using BookingApi.Dto;
using BookingApi.Common;
using BookingApi.Domain;
using BookingApi.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace BookingApi.Tests.Services;

public class BookingServiceTests
{
    private static async Task<TestBookingServiceContext> CreateServiceAsync()
    {
        //create and open connection for in memory db
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var dbContext = new AppDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var logger = NullLogger<BookingService>.Instance;

        var service = new BookingService(dbContext, logger);

        return new TestBookingServiceContext(dbContext, service, connection);
    }
    private sealed class TestBookingServiceContext : IAsyncDisposable
    {
        public TestBookingServiceContext(
            AppDbContext dbContext,
            BookingService service,
            SqliteConnection connection)
        {
            DbContext = dbContext;
            Service = service;
            Connection = connection;
        }

        public AppDbContext DbContext { get; }

        public BookingService Service { get; }

        public SqliteConnection Connection { get; }

        public async ValueTask DisposeAsync()
        {
            await DbContext.DisposeAsync();
            await Connection.DisposeAsync();
        }
    }
    private static async Task<Booking> SeedBookingAsync(
        AppDbContext dbContext,
        int userId,
        string title = "Room 101",
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var now = DateTime.UtcNow;

        var booking = new Booking
        (
            title,
            startDate ?? now.AddHours(1),
            endDate ?? now.AddHours(2),
            userId
        );

        dbContext.Bookings.Add(booking);
        await dbContext.SaveChangesAsync();

        return booking;
    }
    private static async Task<User> SeedUserAsync(
        AppDbContext dbContext,
        string login = "test-user",
        Roles role = Roles.Moderator)
    {
        var user = new User
        (
            login,
            "fake-password-hash",
            role
        );

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        return user;
    }
    [Fact]
    public async Task CreateAsync_ShouldCreateBooking_WhenDataIsValid()
    {
        // Arrange
        await using var context = await CreateServiceAsync();

        await SeedUserAsync(context.DbContext, "admin",Roles.Admin);
        
        var dto = new CreateBookingDto
        ("Room 101",
             DateTime.UtcNow.AddHours(1),
             DateTime.UtcNow.AddHours(2)
        );

        // Act
        var result = await context.Service.CreateAsync(dto, userId: 1);

        // Assert
        Assert.True(result.IsSuccess);

        var bookingFromDb = await context.DbContext.Bookings.SingleAsync();

        Assert.Equal("Room 101", bookingFromDb.Title);
        Assert.Equal(1, bookingFromDb.CreatedByUserId);
    }
    [Fact]
    public async Task CreateAsync_ShouldReturnValidationError_WhenTitleIsEmpty()
    {
        await using var context = await CreateServiceAsync();
        await SeedUserAsync(context.DbContext, "admin",Roles.Admin);

        var dto = new CreateBookingDto(
            "",
            DateTime.UtcNow.AddHours(1),
            DateTime.UtcNow.AddHours(2));
        
        var result = await context.Service.CreateAsync(dto,  userId: 1);
        
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.ValidationError, result.Error);
        
        var bookingsCount = await context.DbContext.Bookings.CountAsync();
        Assert.Equal(0, bookingsCount);
    }
    [Fact]
    public async Task CreateAsync_ShouldReturnValidationError_WhenStartDateIsInPast()
    {
        // Arrange
        await using var context = await CreateServiceAsync();

        var user = await SeedUserAsync(context.DbContext, "admin", Roles.Admin);

        var now = DateTime.UtcNow;

        var dto = new CreateBookingDto(
            "Room 101",
            now.AddHours(-1),
            now.AddHours(2));

        // Act
        var result = await context.Service.CreateAsync(dto, userId: user.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.ValidationError, result.Error);

        var bookingsCount = await context.DbContext.Bookings.CountAsync();

        Assert.Equal(0, bookingsCount);
    }
    [Fact]
    public async Task CreateAsync_ShouldReturnValidationError_WhenEndDateIsInPast()
    {
        // Arrange
        await using var context = await CreateServiceAsync();

        var user = await SeedUserAsync(context.DbContext, "admin", Roles.Admin);

        var now = DateTime.UtcNow;

        var dto = new CreateBookingDto(
            "Room 101",
            now.AddHours(1),
            now.AddHours(-1));

        // Act
        var result = await context.Service.CreateAsync(dto, userId: user.Id);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.ValidationError, result.Error);

        var bookingsCount = await context.DbContext.Bookings.CountAsync();

        Assert.Equal(0, bookingsCount);
    }
    [Fact]
    public async Task CreateAsync_ShouldReturnConflict_WhenBookingTimeOverlaps()
    {
        await using var context = await CreateServiceAsync();

        var user = await SeedUserAsync(context.DbContext, "admin", Roles.Admin);
        var now = DateTime.UtcNow;

        var existingBooking = new Booking(
            "ExistingBooking",
            now.AddHours(1),
            now.AddHours(3),
            user.Id);
        
        context.DbContext.Bookings.Add(existingBooking);
        await context.DbContext.SaveChangesAsync();

        var dto = new CreateBookingDto(
            "Overlapping booking",
            now.AddHours(2),
            now.AddHours(4));
        
        var result = await context.Service.CreateAsync(dto, userId: user.Id);
        
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.Conflict, result.Error);
        
        var bookingsCount = await context.DbContext.Bookings.CountAsync();
        Assert.Equal(1, bookingsCount);
    }
    [Fact]
    public async Task CreateAsync_ShouldCreateBooking_WhenNewBookingStartsAtExistingBookingEnd()
    {
        await using var context = await CreateServiceAsync();
        var user = await SeedUserAsync(context.DbContext, "admin", Roles.Admin);
        
        var now = DateTime.UtcNow;
        var existingStart = now.AddHours(1);
        var existingEnd = now.AddHours(3);
        
        var existingBooking = new Booking(
            "Existing booking",
            existingStart,
            existingEnd,
            user.Id
        );
        
        context.DbContext.Bookings.Add(existingBooking);
        await context.DbContext.SaveChangesAsync();
        
        var dto = new CreateBookingDto(
            "Next booking",
            existingEnd,
            existingEnd.AddHours(1));
        
        var result = await context.Service.CreateAsync(dto, userId: user.Id);
        Assert.True(result.IsSuccess);
        
        var bookings = await context.DbContext.Bookings
            .OrderBy(b => b.StartDate)
            .ToListAsync();
        
        Assert.Equal(2, bookings.Count);
        
        Assert.Equal("Existing booking", bookings[0].Title);
        Assert.Equal("Next booking", bookings[1].Title);
        
        Assert.Equal(existingEnd, bookings[1].StartDate);
    }
    [Fact]
    public async Task CreateAsync_ShouldCreateBooking_WhenNewBookingEndsAtExistingBookingStart()
    {
        // Arrange
        await using var context = await CreateServiceAsync();

        var user = await SeedUserAsync(context.DbContext, "admin", Roles.Admin);

        var now = DateTime.UtcNow;

        var existingStart = now.AddHours(2);
        var existingEnd = now.AddHours(4);

        var existingBooking = new Booking
        (
            "Existing booking",
            existingStart,
            existingEnd,
            user.Id
        );

        context.DbContext.Bookings.Add(existingBooking);
        await context.DbContext.SaveChangesAsync();

        var dto = new CreateBookingDto(
            "Previous booking",
            existingStart.AddHours(-1),
            existingStart);

        // Act
        var result = await context.Service.CreateAsync(dto, userId: user.Id);

        // Assert
        Assert.True(result.IsSuccess);

        var bookings = await context.DbContext.Bookings
            .OrderBy(b => b.StartDate)
            .ToListAsync();

        Assert.Equal(2, bookings.Count);

        Assert.Equal("Previous booking", bookings[0].Title);
        Assert.Equal("Existing booking", bookings[1].Title);

        Assert.Equal(existingStart, bookings[0].EndDate);
    }
    [Fact]
    public async Task GetByIdAsync_ShouldReturnBooking_WhenBookingExists()
    {
        // Arrange
        await using var context = await CreateServiceAsync();

        var user = await SeedUserAsync(context.DbContext, "admin", Roles.Admin);

        var booking = await SeedBookingAsync(
            context.DbContext,
            userId: user.Id,
            title: "Room 101");

        // Act
        var result = await context.Service.GetByIdAsync(booking.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.Equal(booking.Id, result.Value.Id);
        Assert.Equal("Room 101", result.Value.Title);
        Assert.Equal(user.Id, result.Value.CreatedByUserId);
    }
    [Fact]
    public async Task GetByIdAsync_ShouldReturnNotFound_WhenBookingDoesNotExist()
    {
        // Arrange
        await using var context = await CreateServiceAsync();

        var missingBookingId = 999;

        // Act
        var result = await context.Service.GetByIdAsync(missingBookingId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.NotFound, result.Error);
    }
    [Fact]
    public async Task GetAllAsync_ShouldReturnPagedBookingsOrderedByStartDate()
    {
        // Arrange
        await using var context = await CreateServiceAsync();

        var user = await SeedUserAsync(context.DbContext, "admin", Roles.Admin);

        var now = DateTime.UtcNow;

        await SeedBookingAsync(
            context.DbContext,
            userId: user.Id,
            title: "Booking 3",
            startDate: now.AddHours(3),
            endDate: now.AddHours(4));

        await SeedBookingAsync(
            context.DbContext,
            userId: user.Id,
            title: "Booking 1",
            startDate: now.AddHours(1),
            endDate: now.AddHours(2));

        await SeedBookingAsync(
            context.DbContext,
            userId: user.Id,
            title: "Booking 2",
            startDate: now.AddHours(2),
            endDate: now.AddHours(3));

        await SeedBookingAsync(
            context.DbContext,
            userId: user.Id,
            title: "Booking 4",
            startDate: now.AddHours(4),
            endDate: now.AddHours(5));

        // Act
        var result = await context.Service.GetAllAsync(page: 2, pageSize: 2);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.Equal(2, result.Value.Page);
        Assert.Equal(2, result.Value.PageSize);
        Assert.Equal(4, result.Value.TotalCount);
        Assert.Equal(2, result.Value.Items.Count);

        Assert.Equal("Booking 3", result.Value.Items[0].Title);
        Assert.Equal("Booking 4", result.Value.Items[1].Title);
    }
    [Fact]
    public async Task GetAllAsync_ShouldReturnValidationError_WhenPageIsLessThanOne()
    {
        // Arrange
        await using var context = await CreateServiceAsync();

        // Act
        var result = await context.Service.GetAllAsync(page: 0, pageSize: 10);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.ValidationError, result.Error);
    }
    [Fact]
    public async Task GetAllAsync_ShouldReturnValidationError_WhenPageSizeIsGreaterThanOneHundred()
    {
        // Arrange
        await using var context = await CreateServiceAsync();

        // Act
        var result = await context.Service.GetAllAsync(page: 1, pageSize: 101);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.ValidationError, result.Error);
    }
    [Fact]
    public async Task GetMyAsync_ShouldReturnOnlyCurrentUserBookings()
    {
        // Arrange
        await using var context = await CreateServiceAsync();

        var firstUser = await SeedUserAsync(
            context.DbContext,
            login: "first-user");

        var secondUser = await SeedUserAsync(
            context.DbContext,
            login: "second-user");

        var now = DateTime.UtcNow;

        await SeedBookingAsync(
            context.DbContext,
            userId: firstUser.Id,
            title: "First user booking 1",
            startDate: now.AddHours(1),
            endDate: now.AddHours(2));

        await SeedBookingAsync(
            context.DbContext,
            userId: secondUser.Id,
            title: "Second user booking",
            startDate: now.AddHours(3),
            endDate: now.AddHours(4));

        await SeedBookingAsync(
            context.DbContext,
            userId: firstUser.Id,
            title: "First user booking 2",
            startDate: now.AddHours(5),
            endDate: now.AddHours(6));

        // Act
        var result = await context.Service.GetMyAsync(firstUser.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.Equal(2, result.Value.Count);

        Assert.Contains(result.Value, booking => booking.Title == "First user booking 1");
        Assert.Contains(result.Value, booking => booking.Title == "First user booking 2");

        Assert.DoesNotContain(result.Value, booking => booking.Title == "Second user booking");

        Assert.All(result.Value, booking =>
        {
            Assert.Equal(firstUser.Id, booking.CreatedByUserId);
        });
    }
    [Fact]
    public async Task UpdateAsync_ShouldUpdateBooking_WhenModeratorUpdatesOwnBooking()
{
    // Arrange
    await using var context = await CreateServiceAsync();

    var user = await SeedUserAsync(
        context.DbContext,
        login: "moderator",
        role: Roles.Moderator);

    var now = DateTime.UtcNow;

    var booking = await SeedBookingAsync(
        context.DbContext,
        userId: user.Id,
        title: "Old title",
        startDate: now.AddHours(1),
        endDate: now.AddHours(2));

    var dto = new UpdateBookingDto(
        "Updated title",
        now.AddHours(3),
        now.AddHours(4));

    // Act
    var result = await context.Service.UpdateAsync(
        booking.Id,
        dto,
        currentUserId: user.Id,
        "Moderator");

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);

    Assert.Equal(booking.Id, result.Value.Id);
    Assert.Equal("Updated title", result.Value.Title);
    Assert.Equal(dto.StartDate, result.Value.StartDate);
    Assert.Equal(dto.EndDate, result.Value.EndDate);
    Assert.Equal(user.Id, result.Value.CreatedByUserId);

    var bookingFromDb = await context.DbContext.Bookings
        .AsNoTracking()
        .SingleAsync();

    Assert.Equal("Updated title", bookingFromDb.Title);
    Assert.Equal(dto.StartDate, bookingFromDb.StartDate);
    Assert.Equal(dto.EndDate, bookingFromDb.EndDate);
    Assert.Equal(user.Id, bookingFromDb.CreatedByUserId);
}

    [Fact]
    public async Task UpdateAsync_ShouldReturnForbidden_WhenModeratorUpdatesAnotherUserBooking()
{
    // Arrange
    await using var context = await CreateServiceAsync();

    var owner = await SeedUserAsync(
        context.DbContext,
        login: "owner",
        role: Roles.Moderator);

    var anotherUser = await SeedUserAsync(
        context.DbContext,
        login: "another-user",
        role: Roles.Moderator);

    var now = DateTime.UtcNow;

    var booking = await SeedBookingAsync(
        context.DbContext,
        userId: owner.Id,
        title: "Original title",
        startDate: now.AddHours(1),
        endDate: now.AddHours(2));

    var dto = new UpdateBookingDto(
        "Hacked title",
        now.AddHours(3),
        now.AddHours(4));

    // Act
    var result = await context.Service.UpdateAsync(
        booking.Id,
        dto,
        currentUserId: anotherUser.Id,
        userRole: "Moderator");

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal(ErrorCode.Forbidden, result.Error);

    var bookingFromDb = await context.DbContext.Bookings
        .AsNoTracking()
        .SingleAsync();

    Assert.Equal("Original title", bookingFromDb.Title);
    Assert.Equal(now.AddHours(1), bookingFromDb.StartDate);
    Assert.Equal(now.AddHours(2), bookingFromDb.EndDate);
    Assert.Equal(owner.Id, bookingFromDb.CreatedByUserId);
}

    [Fact]
    public async Task UpdateAsync_ShouldUpdateBooking_WhenAdminUpdatesAnotherUserBooking()
{
    // Arrange
    await using var context = await CreateServiceAsync();

    var owner = await SeedUserAsync(
        context.DbContext,
        login: "owner",
        role: Roles.Moderator);

    var admin = await SeedUserAsync(
        context.DbContext,
        login: "admin",
        role: Roles.Admin);

    var now = DateTime.UtcNow;

    var booking = await SeedBookingAsync(
        context.DbContext,
        userId: owner.Id,
        title: "Old title",
        startDate: now.AddHours(1),
        endDate: now.AddHours(2));

    var dto = new UpdateBookingDto(
        "Admin updated title",
        now.AddHours(3),
        now.AddHours(4));

    // Act
    var result = await context.Service.UpdateAsync(
        booking.Id,
        dto,
        currentUserId: admin.Id,
        userRole: "Admin");

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);

    Assert.Equal(booking.Id, result.Value.Id);
    Assert.Equal("Admin updated title", result.Value.Title);
    Assert.Equal(dto.StartDate, result.Value.StartDate);
    Assert.Equal(dto.EndDate, result.Value.EndDate);

    // Владелец брони не должен измениться на админа.
    Assert.Equal(owner.Id, result.Value.CreatedByUserId);

    var bookingFromDb = await context.DbContext.Bookings
        .AsNoTracking()
        .SingleAsync();

    Assert.Equal("Admin updated title", bookingFromDb.Title);
    Assert.Equal(dto.StartDate, bookingFromDb.StartDate);
    Assert.Equal(dto.EndDate, bookingFromDb.EndDate);
    Assert.Equal(owner.Id, bookingFromDb.CreatedByUserId);
}

    [Fact]
    public async Task UpdateAsync_ShouldReturnNotFound_WhenBookingDoesNotExist()
{
    // Arrange
    await using var context = await CreateServiceAsync();

    var user = await SeedUserAsync(
        context.DbContext,
        login: "moderator",
        role: Roles.Moderator);

    var now = DateTime.UtcNow;

    var dto = new UpdateBookingDto(
        "Updated title",
        now.AddHours(1),
        now.AddHours(2));

    var missingBookingId = 999;

    // Act
    var result = await context.Service.UpdateAsync(
        missingBookingId,
        dto,
        currentUserId: user.Id,
        userRole: "Moderator");

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal(ErrorCode.NotFound, result.Error);

    var bookingsCount = await context.DbContext.Bookings.CountAsync();

    Assert.Equal(0, bookingsCount);
}

    [Fact]
    public async Task UpdateAsync_ShouldReturnValidationError_WhenTitleIsEmpty()
{
    // Arrange
    await using var context = await CreateServiceAsync();

    var user = await SeedUserAsync(
        context.DbContext,
        login: "moderator",
        role: Roles.Moderator);

    var now = DateTime.UtcNow;

    var booking = await SeedBookingAsync(
        context.DbContext,
        userId: user.Id,
        title: "Original title",
        startDate: now.AddHours(1),
        endDate: now.AddHours(2));

    var dto = new UpdateBookingDto(
        "",
        now.AddHours(3),
        now.AddHours(4));

    // Act
    var result = await context.Service.UpdateAsync(
        booking.Id,
        dto,
        currentUserId: user.Id,
        userRole: "Moderator");

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal(ErrorCode.ValidationError, result.Error);

    var bookingFromDb = await context.DbContext.Bookings
        .AsNoTracking()
        .SingleAsync();

    Assert.Equal("Original title", bookingFromDb.Title);
    Assert.Equal(now.AddHours(1), bookingFromDb.StartDate);
    Assert.Equal(now.AddHours(2), bookingFromDb.EndDate);
}

    [Fact]
    public async Task UpdateAsync_ShouldReturnValidationError_WhenStartDateIsInPast()
{
    // Arrange
    await using var context = await CreateServiceAsync();

    var user = await SeedUserAsync(
        context.DbContext,
        login: "moderator",
        role: Roles.Moderator);

    var now = DateTime.UtcNow;

    var booking = await SeedBookingAsync(
        context.DbContext,
        userId: user.Id,
        title: "Original title",
        startDate: now.AddHours(1),
        endDate: now.AddHours(2));

    var dto = new UpdateBookingDto(
        "Updated title",
        now.AddHours(-1),
        now.AddHours(2));

    // Act
    var result = await context.Service.UpdateAsync(
        booking.Id,
        dto,
        currentUserId: user.Id,
        userRole: "Moderator");

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal(ErrorCode.ValidationError, result.Error);

    var bookingFromDb = await context.DbContext.Bookings
        .AsNoTracking()
        .SingleAsync();

    Assert.Equal("Original title", bookingFromDb.Title);
    Assert.Equal(now.AddHours(1), bookingFromDb.StartDate);
    Assert.Equal(now.AddHours(2), bookingFromDb.EndDate);
}

    [Fact]
    public async Task UpdateAsync_ShouldReturnValidationError_WhenEndDateIsBeforeStartDate()
{
    // Arrange
    await using var context = await CreateServiceAsync();

    var user = await SeedUserAsync(
        context.DbContext,
        login: "moderator",
        role: Roles.Moderator);

    var now = DateTime.UtcNow;

    var booking = await SeedBookingAsync(
        context.DbContext,
        userId: user.Id,
        title: "Original title",
        startDate: now.AddHours(1),
        endDate: now.AddHours(2));

    var dto = new UpdateBookingDto(
        "Updated title",
        now.AddHours(4),
        now.AddHours(3));

    // Act
    var result = await context.Service.UpdateAsync(
        booking.Id,
        dto,
        currentUserId: user.Id,
        userRole: "Moderator");

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal(ErrorCode.ValidationError, result.Error);

    var bookingFromDb = await context.DbContext.Bookings
        .AsNoTracking()
        .SingleAsync();

    Assert.Equal("Original title", bookingFromDb.Title);
    Assert.Equal(now.AddHours(1), bookingFromDb.StartDate);
    Assert.Equal(now.AddHours(2), bookingFromDb.EndDate);
}

    [Fact]
    public async Task UpdateAsync_ShouldReturnConflict_WhenUpdatedTimeOverlapsAnotherBooking()
{
    // Arrange
    await using var context = await CreateServiceAsync();

    var user = await SeedUserAsync(
        context.DbContext,
        login: "moderator",
        role: Roles.Moderator);

    var now = DateTime.UtcNow;

    var bookingToUpdate = await SeedBookingAsync(
        context.DbContext,
        userId: user.Id,
        title: "Booking to update",
        startDate: now.AddHours(1),
        endDate: now.AddHours(2));

    await SeedBookingAsync(
        context.DbContext,
        userId: user.Id,
        title: "Existing booking",
        startDate: now.AddHours(3),
        endDate: now.AddHours(5));

    var dto = new UpdateBookingDto(
        "Conflicting update",
        now.AddHours(4),
        now.AddHours(6));

    // Act
    var result = await context.Service.UpdateAsync(
        bookingToUpdate.Id,
        dto,
        currentUserId: user.Id,
        userRole: "Moderator");

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal(ErrorCode.Conflict, result.Error);

    var updatedBookingFromDb = await context.DbContext.Bookings
        .AsNoTracking()
        .SingleAsync(b => b.Id == bookingToUpdate.Id);

    Assert.Equal("Booking to update", updatedBookingFromDb.Title);
    Assert.Equal(now.AddHours(1), updatedBookingFromDb.StartDate);
    Assert.Equal(now.AddHours(2), updatedBookingFromDb.EndDate);

    var bookingsCount = await context.DbContext.Bookings.CountAsync();

    Assert.Equal(2, bookingsCount);
}

    [Fact]
    public async Task DeleteAsync_ShouldDeleteBooking_WhenModeratorDeletesOwnBooking()
{
    // Arrange
    await using var context = await CreateServiceAsync();

    var user = await SeedUserAsync(
        context.DbContext,
        login: "moderator",
        role: Roles.Moderator);

    var now = DateTime.UtcNow;

    var booking = await SeedBookingAsync(
        context.DbContext,
        userId: user.Id,
        title: "Booking to delete",
        startDate: now.AddHours(1),
        endDate: now.AddHours(2));

    // Act
    var result = await context.Service.DeleteAsync(
        booking.Id,
        user.Id,
        "Moderator");

    // Assert
    Assert.True(result.IsSuccess);

    var bookingsCount = await context.DbContext.Bookings.CountAsync();

    Assert.Equal(0, bookingsCount);
}

    [Fact]
    public async Task DeleteAsync_ShouldReturnForbidden_WhenModeratorDeletesAnotherUserBooking()
{
    // Arrange
    await using var context = await CreateServiceAsync();

    var owner = await SeedUserAsync(
        context.DbContext,
        login: "owner",
        role: Roles.Moderator);

    var anotherUser = await SeedUserAsync(
        context.DbContext,
        login: "another-user",
        role: Roles.Moderator);

    var now = DateTime.UtcNow;

    var booking = await SeedBookingAsync(
        context.DbContext,
        userId: owner.Id,
        title: "Owner booking",
        startDate: now.AddHours(1),
        endDate: now.AddHours(2));

    // Act
    var result = await context.Service.DeleteAsync(
        booking.Id,
        anotherUser.Id,
        "Moderator");

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal(ErrorCode.Forbidden, result.Error);

    var bookingFromDb = await context.DbContext.Bookings
        .AsNoTracking()
        .SingleAsync();

    Assert.Equal(booking.Id, bookingFromDb.Id);
    Assert.Equal("Owner booking", bookingFromDb.Title);
    Assert.Equal(owner.Id, bookingFromDb.CreatedByUserId);

    var bookingsCount = await context.DbContext.Bookings.CountAsync();

    Assert.Equal(1, bookingsCount);
}

    [Fact]
    public async Task DeleteAsync_ShouldDeleteBooking_WhenAdminDeletesAnotherUserBooking()
{
    // Arrange
    await using var context = await CreateServiceAsync();

    var owner = await SeedUserAsync(
        context.DbContext,
        login: "owner",
        role: Roles.Moderator);

    var admin = await SeedUserAsync(
        context.DbContext,
        login: "admin",
        role: Roles.Admin);

    var now = DateTime.UtcNow;

    var booking = await SeedBookingAsync(
        context.DbContext,
        userId: owner.Id,
        title: "Owner booking",
        startDate: now.AddHours(1),
        endDate: now.AddHours(2));

    // Act
    var result = await context.Service.DeleteAsync(
        booking.Id,
        admin.Id,
        "Admin");

    // Assert
    Assert.True(result.IsSuccess);

    var bookingsCount = await context.DbContext.Bookings.CountAsync();

    Assert.Equal(0, bookingsCount);
}

    [Fact]
    public async Task DeleteAsync_ShouldReturnNotFound_WhenBookingDoesNotExist()
    {
    // Arrange
    await using var context = await CreateServiceAsync();

    var user = await SeedUserAsync(
        context.DbContext,
        login: "moderator",
        role: Roles.Moderator);

    var missingBookingId = 999;

    // Act
    var result = await context.Service.DeleteAsync(
        missingBookingId,
        user.Id,
        "Moderator");

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal(ErrorCode.NotFound, result.Error);

    var bookingsCount = await context.DbContext.Bookings.CountAsync();

    Assert.Equal(0, bookingsCount);
    }   
}