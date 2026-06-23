using Microsoft.AspNetCore.Mvc;
using ChatService.Api.Models;
using ChatService.Api.Services;

namespace ChatService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] LoginDto dto)
        {
            try
            {
                var (success, message, token) = await _authService.RegisterAsync(dto.Username, dto.Password);

                if (!success)
                {
                    return Conflict(new ErrorResponse
                    {
                        StatusCode = 409,
                        Message = message
                    });
                }

                return Ok(new { token });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error during registration");
                throw;
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                var (success, message, token) = await _authService.LoginAsync(dto.Username, dto.Password);

                if (!success)
                {
                    return Unauthorized(new ErrorResponse
                    {
                        StatusCode = 401,
                        Message = message
                    });
                }

                return Ok(new { token });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error during login");
                throw;
            }
        }
    }
}