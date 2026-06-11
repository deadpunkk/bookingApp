using BookingApi.Common;
using BookingApi.Dto;
using BookingApi.Extensions;
using BookingApi.Services;

namespace BookingApi.Endpoints;

public static class UserEndpoints
{
    public static void MapUsersEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/users")
            .WithTags("Users");

        group.MapGet("/", async (IUserService service) =>
        {
            var result = await service.GetAllAsync();
            return Results.Ok(result.Value!.ToDtoList());
        }).RequireAuthorization(policy => policy.RequireRole("Admin"));

        group.MapGet("/{id}", async (int id, IUserService service) =>
        {
            var result = await service.GetByIdAsync(id);
            return result.IsSuccess ? Results.Ok(result.Value!.ToDto()) : result.Error.ToHttpResult();   
        }).RequireAuthorization(policy => policy.RequireRole("Admin"));

        group.MapPost("/", async (IUserService service, CreateUserDto dto) =>
        {
            var result = await service.CreateAsync(dto);
            return result.IsSuccess ? Results.Created($"/users/{result.Value!.Id}", result.Value.ToDto()) 
                                    : result.Error.ToHttpResult();
        }).RequireAuthorization(policy => policy.RequireRole("Admin"));
        
        group.MapPost("/login", async (
            IUserService service,
            IJwtService jwtService,
            LoginDto dto) =>
        {
            var result = await service.LoginAsync(dto);

            if (!result.IsSuccess)
            {
                return result.Error.ToHttpResult();
            }
            
            var token = jwtService.GenerateToken(result.Value!);

            var response = new LoginResponseDto(
                token,
                result.Value!.ToDto());
            
            return Results.Ok(response);
        }).RequireAuthorization(policy => policy.RequireRole("Admin"));
    }
}