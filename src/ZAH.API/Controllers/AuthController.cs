using Microsoft.AspNetCore.Mvc;
using ZAH.Application.DTOs;
using ZAH.Application.Interfaces;

namespace ZAH.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
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

    [HttpGet("me")]
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
