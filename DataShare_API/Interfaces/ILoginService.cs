namespace DataShare_API.Interfaces
{
    public interface ILoginService
    {
        string? Login(string email, string password);
    }
}