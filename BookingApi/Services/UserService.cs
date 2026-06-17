using BookingApi.Common;
using BookingApi.Dto;
using BookingApi.Domain;
using BookingApi.Data;
using Microsoft.EntityFrameworkCore;
namespace BookingApi.Services;

public class UserService : IUserService
{
    //dataBase initialisation
    private readonly AppDbContext _db;
    
    //Ilogger initialisation
    private readonly ILogger<UserService> _logger;
    
    //constructor injection
    public UserService(AppDbContext db, ILogger<UserService> logger)
    {
        _db = db;
        _logger = logger;
    }

    //hashing password
    private static string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password);

                            //main crud//
        
    //Get all users from BD
    public async Task<Result<IReadOnlyList<User>>> GetAllAsync()
    {
        var users = await _db.Users.ToListAsync();
        return Result<IReadOnlyList<User>>.Success(users);
    }
        //get user by id 
    public async Task<Result<User>> GetByIdAsync(int id)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            return Result<User>.Failure(ErrorCode.NotFound);
        }
        return Result<User>.Success(user);
        
    }
        //Create user. Validate, check business logic, hash password and pull in db
    public async Task<Result<User>> CreateAsync(CreateUserDto dto)
    {
        var login = dto.Login.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(login) 
            || string.IsNullOrWhiteSpace(dto.Password) 
            || dto.Password.Length < 8)
        {
            return Result<User>.Failure(ErrorCode.ValidationError);
        }

        var hasConflict = await  _db.Users.AnyAsync(l => l.Login == login);
        if (hasConflict)
        {
            return Result<User>.Failure(ErrorCode.Conflict);
        }
        
        var password = HashPassword(dto.Password);
        var user = new User(login, password, dto.Role);
        
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        
        return Result<User>.Success(user);
    }
    
                //authorisation //
    public async Task<Result<User>> LoginAsync(LoginDto dto)
    {
        var login = dto.Login.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(l => l.Login == login);
        if (user == null)
        {
            return Result<User>.Failure(ErrorCode.NotFound);
        }
        
        bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);

        return isPasswordCorrect ? Result<User>.Success(user) : Result<User>.Failure(ErrorCode.ValidationError); 
    }
}

