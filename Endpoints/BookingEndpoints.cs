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

        
        group.MapGet("/", async (IBookingService bookingService) =>
        {
            var result = await bookingService.GetAllAsync();
            return Results.Ok(result.Value!.ToDtoList());
        });
        
        group.MapGet("/{id}", async (int id, IBookingService bookingService) =>
        {
            var result = await bookingService.GetByIdAsync(id);

            return !result.IsSuccess ? Results.NotFound() 
                : Results.Ok(result.Value!.ToDto());
        });
        
        group.MapPost("/", async (IBookingService bookingService, CreateBookingDto dto) =>
        {
            var result = await bookingService.CreateAsync(dto);
            return result.IsSuccess ? Results.Created($"/bookings/{result.Value!.Id}", result.Value.ToDto()) 
                : result.Error.ToHttpResult();
        });
        
        group.MapPut("/{id}", async (int id, IBookingService bookingService, UpdateBookingDto dto) =>
        {
            var result = await bookingService.UpdateAsync(id, dto);
            return result.IsSuccess ? Results.NoContent() 
                : result.Error.ToHttpResult();
        });
        
        group.MapDelete("/{id}", async (int id, IBookingService bookingService) =>
        {
            var result = await bookingService.DeleteAsync(id);
            return result.IsSuccess ? Results.NoContent() 
                : result.Error.ToHttpResult();
        });
    }
}