using global::Project3.DTO;
using global::Project3.Models;

namespace Project3.Services;

public interface IUserService
{
    Task<(bool IsSuccess, string ErrorMessage, User? User)> CreateUserAsync(CreateUserRequest request);
}

