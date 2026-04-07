using Project3.ViewModels;
using Project3.Models;

namespace Project3.Services;

public interface IUserService
{
    Task<int> CreateUserAsync(User user);

    Task<List<User>> GetAllUsersAsync();

    Task<User> FindById(int id);
}

