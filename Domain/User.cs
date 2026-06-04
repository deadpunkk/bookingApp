using BookingApi.Common;

namespace BookingApi.Domain;

public class User
{
    public int Id { get; set; }
    public string Login { get; set; }  = string.Empty;
    public string PasswordHash { get; set; }  = string.Empty;
    public Roles Role { get; set; }
    
    public User(string login, string passwordHash, Roles role)
    {
        Login = login;
        PasswordHash = passwordHash;
        Role = role;
    }
    
    private  User()
    {
    }
}