using Microsoft.AspNetCore.Mvc;
using Project3.DTOs;
using Project3.Interfaces;

namespace Project3.Controllers
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
            var token = _loginService.Login(request);

            if (token == null)
            {
                return Unauthorized(new { message = "Email ou mot de passe incorrect." });
            }

            return Ok(new { token });
        }
    }
}