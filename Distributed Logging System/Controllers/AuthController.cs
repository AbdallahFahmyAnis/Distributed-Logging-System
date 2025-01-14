using Distributed_Logging_System.Application.DataTransferObject;
using Distributed_Logging_System.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;


namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto model)
        {
            var result = await _userService.RegisterUserAsync(model.Email, model.FullName, model.Password);
            return result ? Ok(new { Message = "User registered successfully" }) : BadRequest("Registration failed");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto model)
        {
            var token = await _userService.LoginUserAsync(model.Email, model.Password);
            return token != null ? Ok(new { Token = token }) : Unauthorized();
        }
    }
}