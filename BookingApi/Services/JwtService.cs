using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using BookingApi.Domain;

namespace BookingApi.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public string GenerateToken(User user)
    {       
            //Creating Claim, unique user info
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Login),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
        };
            
            //Reading info from config to JWT key
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];
        var secretKey = _configuration["Jwt:SecretKey"];
            
            // Creating sec key from config. SymmetricSecurityKey means what key will create once, signing and check
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
            
            //What key use and algorithm
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        //C# object for create jwt entity 
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(20),
            signingCredentials: credentials
            );
            //returning JWT
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}