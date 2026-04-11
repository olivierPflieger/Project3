using Project3.DTO;
using Project3.Models;

namespace Project3.Services;

public interface IUserService
{
    Task<int> CreateUserAsync(CreateUserRequest createUserRequest);
    
    Task<User> FindById(int id);
}

