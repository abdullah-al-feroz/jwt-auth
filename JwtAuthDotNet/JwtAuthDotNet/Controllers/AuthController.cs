using JwtAuthDotNet.Dtos.Request;
using JwtAuthDotNet.Dtos.Response;
using JwtAuthDotNet.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuthDotNet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<ActionResult<UserResponseDto>> Register(UserRegisterDto request)
        {
            var user = await authService.RegisterAsync(request);
            if (user is null)
                return BadRequest("Username already exists.");

            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<ActionResult<TokenResponseDto>> Login(UserLoginDto request)
        {
            var result = await authService.LoginAsync(request);
            if (result is null)
                return BadRequest("Invalid username or password.");

            return Ok(result);
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<TokenResponseDto>> RefreshToken(RefreshTokenRequestDto request)
        {
            var result = await authService.RefreshTokensAsync(request);
            if (result is null || result.AccessToken is null || result.RefreshToken is null)
                return Unauthorized("Invalid refresh token.");

            return Ok(result);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var success = await authService.SendResetPasswordEmailAsync(dto);
            if (!success) return NotFound("User not found.");
            return Ok("Password reset link sent.");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var success = await authService.ResetPasswordAsync(dto);
            if (!success) return BadRequest("Invalid or expired token.");
            return Ok("Password has been reset.");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult AuthenticatedOnlyEndpoint()
        {
            return Ok("You are authenticated!");
        }

        [Authorize(Roles = "Admin")]
        [Authorize(Policy = "AdminPolicy")]
        [HttpGet("admin-only")]
        public IActionResult AdminOnlyEndpoint()
        {
            return Ok("You are and admin!");
        }

        //[Authorize(Roles = "Admin,Manager")]
        //public IActionResult MultiRoleEndpoint()
        //{
        //    return Ok("Admin or Manager allowed");
        //}

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequestDto dto)
        {
            var success = await authService.LogoutAsync(dto);
            if (!success) return NotFound("User not found or already logged out.");
            return Ok("Logged out successfully.");
        }
    }
}
