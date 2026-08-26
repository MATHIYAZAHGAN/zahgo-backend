using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZAH.Application.DTOs;
using ZAH.Application.Interfaces;

namespace ZAH.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IOtpService _otpService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        IOtpService otpService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _otpService = otpService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        _logger.LogInformation("User registration attempt for email: {Email}", dto.Email);

        var result = await _authService.RegisterAsync(dto);

        if (!result.Success)
        {
            _logger.LogWarning("Registration failed for email: {Email}. Reason: {Message}", dto.Email, result.Message);
            return BadRequest(new { success = false, message = result.Message });
        }

        _logger.LogInformation("User registered successfully: {Email}", dto.Email);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        _logger.LogInformation("Login attempt for email: {Email}", dto.Email);

        var result = await _authService.LoginAsync(dto);

        if (!result.Success)
        {
            _logger.LogWarning("Login failed for email: {Email}", dto.Email);
            return Unauthorized(new { success = false, message = result.Message });
        }

        _logger.LogInformation("User logged in successfully: {Email}", dto.Email);
        return Ok(result);
    }

    [HttpPost("send-otp")]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpDto dto)
    {
        _logger.LogInformation("OTP request for phone: {Phone}", dto.Phone);

        var result = await _otpService.SendOtpAsync(dto);

        if (!result.Success)
        {
            _logger.LogWarning("OTP send failed for phone: {Phone}. Reason: {Message}", dto.Phone, result.Message);
            return BadRequest(result);
        }

        _logger.LogInformation("OTP sent successfully to phone: {Phone}", dto.Phone);
        return Ok(result);
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
    {
        _logger.LogInformation("OTP verification attempt for phone: {Phone}", dto.Phone);

        var result = await _otpService.VerifyOtpAsync(dto);

        if (!result.Success)
        {
            _logger.LogWarning("OTP verification failed for phone: {Phone}", dto.Phone);
            return Unauthorized(result);
        }

        _logger.LogInformation("OTP verified successfully for phone: {Phone}", dto.Phone);
        return Ok(result);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        _logger.LogInformation("Password reset request for email: {Email}", dto.Email);

        var result = await _authService.ForgotPasswordAsync(dto);

        // Always return success to prevent email enumeration
        _logger.LogInformation("Password reset request processed for email: {Email}", dto.Email);
        return Ok(new { success = true, message = "If an account exists with this email, you will receive reset instructions." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        _logger.LogInformation("Password reset attempt with token");

        var result = await _authService.ResetPasswordAsync(dto);

        if (!result.Success)
        {
            _logger.LogWarning("Password reset failed");
            return BadRequest(result);
        }

        _logger.LogInformation("Password reset successful");
        return Ok(result);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
    {
        _logger.LogInformation("Token refresh attempt");

        var result = await _authService.RefreshTokenAsync(dto.RefreshToken);

        if (!result.Success)
        {
            _logger.LogWarning("Token refresh failed");
            return Unauthorized(new { success = false, message = result.Message });
        }

        return Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutDto dto)
    {
        _logger.LogInformation("Logout request");

        var result = await _authService.LogoutAsync(dto);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        _logger.LogInformation("User logged out successfully");
        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirst("userId")?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { success = false, message = "Invalid or missing token" });
        }

        var result = await _authService.GetUserByIdAsync(userId);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }
}
