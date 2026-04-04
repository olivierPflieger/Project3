using Project3.DTOs;

namespace Project3.Interfaces
{
    public interface ILoginService
    {
        string? Login(LoginRequest request);
    }
}