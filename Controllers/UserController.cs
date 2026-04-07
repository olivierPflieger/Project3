using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project3.ViewModels;
using Project3.Models;
using Project3.Services;

namespace Project3.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;    

    public UsersController(IUserService userService)
    {
        _userService = userService;        
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserViewModel request)
    {
        try
        {
            User user = new User
            {
                Email = request.Email,
                Password = request.Password
            };

            await _userService.CreateUserAsync(user);
                return CreatedAtAction(nameof(CreateUser), new { message = "Utilisateur correctement ajouté" });
        }
        catch (ArgumentException ex)
        {
            string errorMessage = $"{ex.Message}";
            return Conflict(new { message = errorMessage });
        }        
        catch (Exception ex)
        {
            string errorMessage = $"Une erreur serveur s'est produite: {ex.Message}";
            return StatusCode(500, new { message = errorMessage });
        }                
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAllUsers()
    {
        try
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            string errorMessage = $"Une erreur s'est produite durant la lecture des utilisateurs: {ex.Message}";
            return StatusCode(500, new { message = errorMessage });
        }
    }
}
