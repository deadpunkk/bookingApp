using System.Security.Claims;
using BookingApi.Dto;
using BookingApi.Extensions;
using BookingApi.Services;
namespace BookingApi.Endpoints;

public static class BookingEndpoints
{
    
    public static void MapBookingEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/bookings")
            .WithTags("Bookings");
        
        group.MapGet("/", async (
            IBookingService bookingService,
            int page = 1,
            int pageSize = 10) =>
        {
            var result = await bookingService.GetAllAsync(page, pageSize);

            if (!result.IsSuccess)
            {
                return result.Error.ToHttpResult();
            }

            var pagedBookings = result.Value!;

            var response = new PagedResult<BookingDto>(
                pagedBookings.Items.ToDtoList(),
                pagedBookings.Page,
                pagedBookings.PageSize,
                pagedBookings.TotalCount);

            return Results.Ok(response);
        }).RequireAuthorization();
        
        group.MapGet("/my", async (
                HttpContext context,
                IBookingService bookingService) =>
            {
                var userId = int.Parse(
                    context.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                var result = await bookingService.GetMyAsync(userId);

                return Results.Ok(result.Value!.ToDtoList());
            })
            .RequireAuthorization();
        
        group.MapGet("/{id}", async (int id, IBookingService bookingService) =>
        {
            var result = await bookingService.GetByIdAsync(id);

            return !result.IsSuccess ? Results.NotFound() 
                : Results.Ok(result.Value!.ToDto());
        }).RequireAuthorization();
        
        group.MapPost("/", async (IBookingService bookingService, CreateBookingDto dto, HttpContext context) =>
        {
            var userId = int.Parse(
                context.User.FindFirstValue(
                    ClaimTypes.NameIdentifier)!);
            
            var result = await bookingService.CreateAsync(dto, userId);
            return result.IsSuccess ? Results.Created($"/bookings/{result.Value!.Id}", result.Value.ToDto()) 
                : result.Error.ToHttpResult();
        }).RequireAuthorization(policy => 
                policy.RequireRole("Admin", "Moderator"));
        
        group.MapPut("/{id}", async (int id, IBookingService bookingService, UpdateBookingDto dto, HttpContext context) =>
        {
            var userId = int.Parse(
                context.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var userRole = context.User.FindFirstValue(ClaimTypes.Role);
            
            var result = await bookingService.UpdateAsync(id, dto, userId, userRole!);
            return result.IsSuccess ? Results.NoContent() 
                : result.Error.ToHttpResult();
        }).RequireAuthorization(policy =>
                policy.RequireRole("Admin", "Moderator"));
        
        group.MapDelete("/{id}", async (int id, IBookingService bookingService, HttpContext context) =>
        {
            var userRole = context.User.FindFirstValue(ClaimTypes.Role);
            var userId = int.Parse(
                context.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            var result = await bookingService.DeleteAsync(id, userId, userRole!);
            return result.IsSuccess ? Results.NoContent() 
                : result.Error.ToHttpResult();
        }).RequireAuthorization(policy =>
            policy.RequireRole("Admin", "Moderator"));
    }
}