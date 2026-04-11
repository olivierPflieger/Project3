using Microsoft.AspNetCore.Mvc;
using DataShare_API.DTO;
using DataShare_API.Interfaces;

namespace DataShare_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly ILoginService _loginService;

        public LoginController(ILoginService loginService)
        {
            _loginService = loginService;
        }

        [HttpPost()]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            try
            {
                var token = _loginService.Login(request.Email, request.Password);
                
                if (!string.IsNullOrEmpty(token))
                {
                    return Ok(new { token });
                }
                else
                {
                    return Unauthorized(new { message = "Email ou mot de passe incorrect." });
                }
            }            
            catch (Exception ex)
            {
                string errorMessage = $"Une erreur serveur s'est produite: {ex.Message}";
                return StatusCode(500, new { message = errorMessage });
            }
        }
    }
}