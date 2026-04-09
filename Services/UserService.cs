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

    public async Task<int> CreateUserAsync(CreateUserRequest createUserRequest)
    {        
        if (await _context.Users.AnyAsync(u => u.Email == createUserRequest.Email))
        {
            throw new ArgumentException("Un utilisateur existe déjà avec cet email");
        }

        // Encrypt password
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(createUserRequest.Password);

        var userToCreated = new User
        {
            Email = createUserRequest.Email,
            Password = passwordHash
        };

        _context.Users.Add(userToCreated);
        return await _context.SaveChangesAsync();
    }

    public async Task<User> FindById(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    // TEMP !! TO DELETE
    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _context.Users.ToListAsync();
    }
}