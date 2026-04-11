using Microsoft.EntityFrameworkCore;
using DataShare_API.DTO;
using DataShare_API.Exceptions;
using DataShare_API.Models;
using System.Net;

namespace DataShare_API.Services;

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
            throw new CustomDatashareException(HttpStatusCode.Conflict, "Un utilisateur existe déjà avec cet email");
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
}