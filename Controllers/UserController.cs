using Microsoft.AspNetCore.Mvc;
using Project3.DTO;
using Project3.Models;
using Project3.Services;

namespace Project3.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly AppDbContext _context;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (isSuccess, errorMessage, createdUser) = await _userService.CreateUserAsync(request);

        if (!isSuccess)
        {
            return Conflict(new { message = errorMessage });
        }

        return CreatedAtAction(nameof(CreateUser), new { id = createdUser!.Id }, new { message = "User correctly created" });
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }
}
