using Microsoft.AspNetCore.Mvc;
using Project3.DTO;
using Project3.Exceptions;
using Project3.Services;
using System.Net;

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
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest createUserRequest)
    {
        try
        {            
            await _userService.CreateUserAsync(createUserRequest);
                return CreatedAtAction(nameof(CreateUser), new { message = "Utilisateur correctement ajouté" });
        }
        catch (CustomDatashareException ex)
        {
            return ResolveCustomException(ex);
        }        
        catch (Exception ex)
        {
            string errorMessage = $"Une erreur serveur s'est produite: {ex.Message}";
            return StatusCode(500, new { message = errorMessage });
        }                
    }
        
    private IActionResult ResolveCustomException(CustomDatashareException ex)
    {
        switch (ex.Code)
        {
            case HttpStatusCode.NotFound:
                return NotFound(new { message = ex.Message });
            case HttpStatusCode.Conflict:
                return Conflict(new { message = ex.Message });
            case HttpStatusCode.BadRequest:
                return BadRequest(new { message = ex.Message });
            default:
                return StatusCode(500, new { message = ex.Message });
        }
    }
}
