using Microsoft.EntityFrameworkCore;
using Project3.Models;
using Project3.DTO;

namespace Project3.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(bool IsSuccess, string ErrorMessage, User? User)> CreateUserAsync(CreateUserRequest request)
    {        
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return (false, "A user is already existing with this email", null);
        }

        // Encrypt password
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var newUser = new User
        {
            Email = request.Email,
            Password = passwordHash
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        return (true, string.Empty, newUser);
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _context.Users.ToListAsync();
    }
}