using global::Project3.DTO;
using global::Project3.Models;
using Project3.DTOs;

namespace Project3.Services;

public interface IUserService
{
    Task<(bool IsSuccess, string ErrorMessage, CreateUserResponse? UserResponse)> CreateUserAsync(CreateUserRequest request);
    Task<List<User>> GetAllUsersAsync();
}

