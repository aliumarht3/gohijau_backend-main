using GoHijauBackend.Application.Interfaces.Services;
using GoHijauBackend.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace GoHijauBackend.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly RegisterUserService _registerService;
        private readonly LoginUserService _loginService;
        private readonly IPasswordResetService _passwordResetService;

        public AuthController(RegisterUserService register, LoginUserService login, IPasswordResetService passwordResetService)
        {
            _registerService = register;
            _loginService = login;
            _passwordResetService = passwordResetService;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] SignupRequest req)
        {
            var result = await _registerService.ExecuteAsync(req.Email, req.Name, req.Password, req.Phone);
            return result.IsSuccess ? Ok() : BadRequest(result.Error);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var result = await _loginService.getAccessTokenUsingEmailPassword(req.Email, req.Password);

            if (!result.IsSuccess) return Unauthorized(result.Error);

            var tokens = result.Value;
            return Ok(new
            {
                accessToken = tokens.AccessToken,
                refreshToken = tokens.RefreshToken
            });
        }

        [HttpPost("refreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var result = await _loginService.getAccessTokenUsingRefreshToken(request.RefreshToken);

            if (!result.IsSuccess) return Unauthorized(result.Error);

            var tokens = result.Value;
            return Ok(new
            {
                accessToken = tokens.AccessToken,
                refreshToken = tokens.RefreshToken
            });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email)) return BadRequest(new { message = "Email is required." });

            var result = await _passwordResetService.GenerateResetToken(request.Email, request.Source);

            // Always return 200 to avoid revealing whether email exists
            if (!result.IsSuccess)
                return Ok(new { message = "If the email exists, you will receive reset instructions." });

            return Ok(new { message = "If the email exists, you will receive reset instructions." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.NewPassword))
                return BadRequest(new { message = "Token and new password are required." });

            var result = await _passwordResetService.ResetPassword(request.Token, request.NewPassword);
            if (!result.IsSuccess) return BadRequest(new { message = result.Error });

            return Ok(new { message = "Password reset successfully." });
        }

        [HttpGet("validate-reset-token")]
        public async Task<IActionResult> ValidateResetToken([FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return BadRequest(new { message = "Token is required." });

            var result = await _passwordResetService.ValidateResetToken(token);

            if (!result.IsSuccess)
                return BadRequest(new { message = "Invalid or expired token." });

            return Ok(new { message = "Token is valid." });
        }
    }

    public record SignupRequest(string Email, string Name, string Password, string Phone);
    public record LoginRequest(string Email, string Password);
    public record RefreshTokenRequest(string RefreshToken);
    public record ForgotPasswordRequest(string Email, string Source);
    public record ResetPasswordRequest(string Token, string NewPassword);
}
