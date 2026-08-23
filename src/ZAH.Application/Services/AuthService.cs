using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ZAH.Application.DTOs;
using ZAH.Application.Interfaces;
using ZAH.Domain.Entities;
using ZAH.Domain.Enums;
using ZAH.Shared.Responses;

namespace ZAH.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public AuthService(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto dto)
    {
        // Check if user already exists
        var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
        if (existingUser != null)
        {
            return ApiResponse<AuthResponseDto>.FailureResponse("User with this email already exists");
        }

        // Hash password
        var passwordHash = HashPassword(dto.Password);

        // Create new user
        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            Phone = dto.Phone,
            PasswordHash = passwordHash,
            Role = UserRole.Customer,
            RewardPoints = 200, // Welcome bonus
            IsEmailVerified = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user);

        // Generate tokens
        var (accessToken, expiresAt) = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

        // Save refresh token
        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id!,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        };
        await _userRepository.SaveRefreshTokenAsync(refreshTokenEntity);

        var response = new AuthResponseDto
        {
            UserId = user.Id!,
            Name = user.Name,
            Email = user.Email,
            Phone = user.Phone,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            RewardPoints = user.RewardPoints
        };

        return ApiResponse<AuthResponseDto>.SuccessResponse(response, "User registered successfully");
    }

    public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto dto)
    {
        // Find user by email
        var user = await _userRepository.GetByEmailAsync(dto.Email);
        if (user == null)
        {
            return ApiResponse<AuthResponseDto>.FailureResponse("Invalid email or password");
        }

        // Verify password
        if (!VerifyPassword(dto.Password, user.PasswordHash))
        {
            return ApiResponse<AuthResponseDto>.FailureResponse("Invalid email or password");
        }

        // Generate tokens
        var (accessToken, expiresAt) = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

        // Save refresh token
        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id!,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        };
        await _userRepository.SaveRefreshTokenAsync(refreshTokenEntity);

        var response = new AuthResponseDto
        {
            UserId = user.Id!,
            Name = user.Name,
            Email = user.Email,
            Phone = user.Phone,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            RewardPoints = user.RewardPoints
        };

        return ApiResponse<AuthResponseDto>.SuccessResponse(response, "Login successful");
    }

    public async Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(string refreshToken)
    {
        var storedToken = await _userRepository.GetRefreshTokenAsync(refreshToken);
        
        if (storedToken == null || storedToken.ExpiresAt < DateTime.UtcNow || storedToken.IsRevoked)
        {
            return ApiResponse<AuthResponseDto>.FailureResponse("Invalid or expired refresh token");
        }

        var user = await _userRepository.GetByIdAsync(storedToken.UserId);
        if (user == null)
        {
            return ApiResponse<AuthResponseDto>.FailureResponse("User not found");
        }

        // Generate new tokens
        var (accessToken, expiresAt) = GenerateAccessToken(user);
        var newRefreshToken = GenerateRefreshToken();

        // Revoke old token
        storedToken.IsRevoked = true;
        await _userRepository.UpdateRefreshTokenAsync(storedToken);

        // Save new refresh token
        var newRefreshTokenEntity = new RefreshToken
        {
            UserId = user.Id!,
            Token = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        };
        await _userRepository.SaveRefreshTokenAsync(newRefreshTokenEntity);

        var response = new AuthResponseDto
        {
            UserId = user.Id!,
            Name = user.Name,
            Email = user.Email,
            Phone = user.Phone,
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = expiresAt,
            RewardPoints = user.RewardPoints
        };

        return ApiResponse<AuthResponseDto>.SuccessResponse(response, "Token refreshed successfully");
    }

    public async Task<ApiResponse<object>> GetUserByIdAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return ApiResponse<object>.FailureResponse("User not found");
        }

        var userDto = new
        {
            user.Id,
            user.Name,
            user.Email,
            user.Phone,
            user.RewardPoints,
            user.Role,
            user.IsEmailVerified,
            Addresses = user.Addresses
        };

        return ApiResponse<object>.SuccessResponse(userDto, "User retrieved successfully");
    }

    private (string token, DateTime expiresAt) GenerateAccessToken(User user)
    {
        var jwtSecret = _configuration["JWT:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
        var jwtIssuer = _configuration["JWT:Issuer"] ?? "ZAH.API";
        var jwtAudience = _configuration["JWT:Audience"] ?? "ZAH.Client";

        var claims = new[]
        {
            new Claim("userId", user.Id!),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddHours(24);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    private string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
