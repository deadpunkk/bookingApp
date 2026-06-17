using BookingApi.Common;
using BookingApi.Domain;
using BookingApi.Data;
using BookingApi.Dto;
using Microsoft.EntityFrameworkCore;

namespace BookingApi.Services;

public class BookingService : IBookingService
{
    //dataBase initialisation
    private readonly AppDbContext _db;
    
    //Ilogger initialisation
    private readonly ILogger<BookingService> _logger;
    
    //constructor injection
    public BookingService(AppDbContext db, ILogger<BookingService> logger)
    {
        _db = db;
        _logger = logger;
    }
    
    
    //validation method 
    private static bool IsValidBookingData(string title, DateTime startDate, DateTime endDate)
    {
        var now = DateTime.UtcNow;
        if (string.IsNullOrWhiteSpace(title)
            || startDate < now
            || endDate <= startDate)
        {
            return false;
        }
        return true;
    }
    
    //work with db, validation and Result<T> class => endpoint isolation logic 
    public async Task<Result<PagedResult<Booking>>> GetAllAsync(int page, int pageSize)
    {
        _logger.LogInformation("Getting bookings. Page: {Page}, PageSize: {PageSize}",
           page,
           pageSize);

        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            _logger.LogWarning(
                "Getting bookings failed. Invalid pagination parameters. Page: {Page}, PageSize: {PageSize}",
                page,
                pageSize);

            return Result<PagedResult<Booking>>.Failure(ErrorCode.ValidationError);
        }
        
        var totalCount = await _db.Bookings.CountAsync();
        
        var bookings = await _db.Bookings
            .OrderBy(b => b.StartDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        var result = new PagedResult<Booking>(
            bookings,
            page,
            pageSize,
            totalCount);
        
        _logger.LogInformation(
            "Returning bookings. Page: {Page}, PageSize: {PageSize}, TotalCount: {TotalCount}",
            page,
            pageSize,
            totalCount);

        return Result<PagedResult<Booking>>.Success(result);
    }
    
    public async Task<Result<Booking>> GetByIdAsync(int id)
    {
        _logger.LogInformation($"Getting booking by ID. (ID: {id})");
        var booking = await _db.Bookings.FirstOrDefaultAsync(item => item.Id == id);
        if (booking == null)
        {
            _logger.LogError("Getting booking by ID failed. Not found");
            return Result<Booking>.Failure(ErrorCode.NotFound);
        }
        _logger.LogInformation("Returning booking with ID {BookingID}", booking.Id);
        return Result<Booking>.Success(booking);
    }

    public async Task<Result<Booking>> CreateAsync(CreateBookingDto dto, int userId)
    {
        _logger.LogInformation("Creating booking..");
        
        if (!IsValidBookingData(dto.Title, dto.StartDate, dto.EndDate))
        {
            _logger.LogWarning(
                "Creating booking failed. Validation error. Title: {Title}, StartDate: {StartDate}, EndDate: {EndDate}",
                dto.Title,
                dto.StartDate,
                dto.EndDate);
            
            return Result<Booking>.Failure(ErrorCode.ValidationError);
        }

        var hasConflict = await _db.Bookings.AnyAsync(existing => 
            existing.StartDate < dto.EndDate
            && dto.StartDate < existing.EndDate);
    
        if (hasConflict)
        {
            _logger.LogWarning(
                "Creating booking failed. Time conflict. StartDate: {StartDate}, EndDate: {EndDate}",
                dto.StartDate,
                dto.EndDate);
            
            return Result<Booking>.Failure(ErrorCode.Conflict);
        }
        
        _logger.LogInformation(
            "Creating booking with title {Title}, start {StartDate}, end {EndDate}",
            dto.Title,
            dto.StartDate,
            dto.EndDate);
        
        var booking = new Booking(dto.Title, dto.StartDate, dto.EndDate, userId);
        
        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();
        
        _logger.LogInformation(
            "Booking created successfully with id {BookingId}",
            booking.Id);
        
        return Result<Booking>.Success(booking);
    }
    public async Task<Result<Booking>> UpdateAsync(int id, UpdateBookingDto dto, int currentUserId, string userRole)
    {
        _logger.LogInformation("Updating booking with ID {BookingId}", id);
        
        if (!IsValidBookingData(dto.Title, dto.StartDate, dto.EndDate))
        {
            _logger.LogWarning(
                "Creating booking failed. Validation error. Title: {Title}, StartDate: {StartDate}, EndDate: {EndDate}",
                dto.Title,
                dto.StartDate,
                dto.EndDate);
            
            return Result<Booking>.Failure(ErrorCode.ValidationError);
        }
        
        var booking = await _db.Bookings.FirstOrDefaultAsync(item => item.Id == id);
        if (booking == null)
        {
            _logger.LogError("Getting booking by ID failed. Not found");
            
            return Result<Booking>.Failure(ErrorCode.NotFound);
        }

        if (userRole == Roles.Moderator.ToString()
            && currentUserId != booking.CreatedByUserId)
        {
            return Result<Booking>.Failure(ErrorCode.Forbidden);
        }
        
        
        var hasConflict = await _db.Bookings.AnyAsync(existing =>
            existing.Id != id
            && existing.StartDate < dto.EndDate
            && dto.StartDate < existing.EndDate);
    
        if (hasConflict)
        {
            _logger.LogWarning(
                "Creating booking failed. Time conflict. StartDate: {StartDate}, EndDate: {EndDate}",
                dto.StartDate,
                dto.EndDate);
            
            return Result<Booking>.Failure(ErrorCode.Conflict);
        }
    
        booking.Title = dto.Title;
        booking.StartDate = dto.StartDate;
        booking.EndDate = dto.EndDate;
        await _db.SaveChangesAsync();
        
        _logger.LogInformation("Booking updated successfully with id {BookingId}",
            booking.Id);
        return Result<Booking>.Success(booking);
    }

    public async Task<Result<bool>> DeleteAsync(int id, int currentUserId, string userRole)
    {
        
        _logger.LogInformation("Deleting booking with ID {BookingId}", id);
        var booking = await _db.Bookings.FirstOrDefaultAsync(item => item.Id == id);
        if (booking == null)
        {
            _logger.LogError("Getting booking by ID failed. Not found");
            return Result<bool>.Failure(ErrorCode.NotFound);
        }
        
        if (userRole == Roles.Moderator.ToString()
            && currentUserId != booking.CreatedByUserId)
        {
            return Result<bool>.Failure(ErrorCode.Forbidden);
        }
        
        _db.Bookings.Remove(booking);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Booking deleted successfully");
        return Result<bool>.Success(true);
    }

    public async Task<Result<IReadOnlyList<Booking>>> GetMyAsync(int userId)
    {
        var bookings = await _db.Bookings
            .Where(b => b.CreatedByUserId == userId)
            .ToListAsync();

        return Result<IReadOnlyList<Booking>>.Success(bookings);
    }
    
}