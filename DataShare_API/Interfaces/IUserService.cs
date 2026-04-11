using DataShare_API.DTO;
using DataShare_API.Models;

namespace DataShare_API.Services;

public interface IUserService
{
    Task<int> CreateUserAsync(CreateUserRequest createUserRequest);
    
    Task<User> FindById(int id);
}

