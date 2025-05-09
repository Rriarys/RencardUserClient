using Microsoft.AspNetCore.Mvc;
using RencardUserClient.DTOs.Auth;
using RencardUserClient.Interfaces;

namespace RencardUserClient.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService authService)
        {
            _auth = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest dto)
        {
            var result = await _auth.RegisterAsync(dto);
            if (!result.Success)
                return BadRequest(result.Errors);
            return Ok(result.Token);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest dto)
        {
            var result = await _auth.LoginAsync(dto);
            if (!result.Success)
                return Unauthorized(result.Errors);
            return Ok(result.Token);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _auth.LogoutAsync();
            return NoContent();
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest dto)
        {
            var result = await _auth.RefreshTokenAsync(dto);
            if (!result.Success)
                return Unauthorized(result.Errors);
            return Ok(result.Token);
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest dto)
        {
            var result = await _auth.ChangePasswordAsync(dto);
            if (!result.Success)
                return BadRequest(result.Errors);
            return NoContent();
        }
    }
}