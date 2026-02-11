using Microsoft.AspNetCore.Mvc;
using Models.DTO.Auth;
using Service.Services.Interfaces;

namespace API.Controllers
{
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var result = await _authService.LoginAsync(request);
            return HandleResult(result, "Login successful");
        }
    }
}
