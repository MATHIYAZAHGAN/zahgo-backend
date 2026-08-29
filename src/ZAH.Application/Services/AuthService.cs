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
using ZAH.Domain.Interfaces;
using ZAH.Shared.Responses;

namespace ZAH.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordResetRepository _passwordResetRepository;
    private readonly IConfiguration _configuration;

    public AuthService(
        IUserRepository userRepository,
        IPasswordResetRepository passwordResetRepository,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _passwordResetRepository = passwordResetRepository;
        _configuration = configuration;
    }

    public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto dto)
    {
        // Check if user already exists
        var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
        if (existingUser != null)
        {
            return ApiResponse<AuthResponseDto>.FailureResponse("An account with this email already exists.");
        }

        // Check phone uniqueness if provided
        if (!string.IsNullOrEmpty(dto.Phone))
        {
            var existingPhone = await _userRepository.GetByPhoneAsync(dto.Phone);
            if (existingPhone != null)
            {
                return ApiResponse<AuthResponseDto>.FailureResponse("An account with this phone number already exists.");
            }
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
            IsPhoneVerified = false,
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

        // Check if account is locked
        if (user.LockedUntil.HasValue && user.LockedUntil.Value > DateTime.UtcNow)
        {
            var minutesLeft = (int)(user.LockedUntil.Value - DateTime.UtcNow).TotalMinutes;
            return ApiResponse<AuthResponseDto>.FailureResponse($"Account is temporarily locked. Please try again in {minutesLeft} minutes.");
        }

        // Check if account is active
        if (!user.IsActive)
        {
            return ApiResponse<AuthResponseDto>.FailureResponse("Account has been deactivated. Please contact support.");
        }

        // Verify password
        if (!VerifyPassword(dto.Password, user.PasswordHash))
        {
            // Increment failed login attempts
            user.FailedLoginAttempts++;

            // Lock account after 5 failed attempts
            if (user.FailedLoginAttempts >= 5)
            {
                user.LockedUntil = DateTime.UtcNow.AddMinutes(30);
                await _userRepository.UpdateAsync(user);
                return ApiResponse<AuthResponseDto>.FailureResponse("Too many failed login attempts. Account locked for 30 minutes.");
            }

            await _userRepository.UpdateAsync(user);
            return ApiResponse<AuthResponseDto>.FailureResponse("Invalid email or password");
        }

        // Reset failed login attempts on successful login
        user.FailedLoginAttempts = 0;
        user.LockedUntil = null;
        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

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

    public async Task<ApiResponse<AuthResponseDto>> GoogleLoginAsync(GoogleAuthDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            return ApiResponse<AuthResponseDto>.FailureResponse("Email is required for Google login.");
        }

        var user = await _userRepository.GetByEmailAsync(dto.Email);
        if (user == null)
        {
            // Register new user authenticated via Google
            user = new User
            {
                Name = !string.IsNullOrWhiteSpace(dto.Name) ? dto.Name : dto.Email.Split('@')[0],
                Email = dto.Email,
                PasswordHash = string.Empty, // Google user
                Role = UserRole.Customer,
                RewardPoints = 200, // Welcome bonus
                IsEmailVerified = true,
                IsPhoneVerified = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _userRepository.CreateAsync(user);
        }
        else
        {
            // Existing user login via Google
            user.IsEmailVerified = true;
            user.LastLoginAt = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(dto.Name) && (string.IsNullOrWhiteSpace(user.Name) || user.Name.StartsWith("User ")))
            {
                user.Name = dto.Name;
            }
            await _userRepository.UpdateAsync(user);
        }

        // Generate JWT tokens
        var (accessToken, expiresAt) = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

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

        return ApiResponse<AuthResponseDto>.SuccessResponse(response, "Logged in via Google successfully");
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
            user.IsPhoneVerified,
            Addresses = user.Addresses
        };

        return ApiResponse<object>.SuccessResponse(userDto, "User retrieved successfully");
    }

    public Task<User?> GetUserEntityByIdAsync(string userId) => _userRepository.GetByIdAsync(userId);

    public Task<User> UpdateUserAsync(User user) => _userRepository.UpdateAsync(user);

    public async Task<ApiResponse<object>> ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        // Find user by email
        var user = await _userRepository.GetByEmailAsync(dto.Email);
        
        // Always return success to prevent email enumeration
        if (user == null)
        {
            return ApiResponse<object>.SuccessResponse(null, "If an account exists with this email, you will receive reset instructions.");
        }

        // Generate secure reset token
        var resetToken = GenerateSecureToken();

        // Create password reset token entity
        var passwordResetToken = new PasswordResetToken
        {
            UserId = user.Id!,
            Token = resetToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1), // 1 hour expiration
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _passwordResetRepository.CreateAsync(passwordResetToken);

        // TODO: Send email with reset link
        // await _emailService.SendPasswordResetEmail(user.Email, resetToken);
        // For now, log the token (REMOVE IN PRODUCTION)
        Console.WriteLine($"Password reset token for {user.Email}: {resetToken}");
        Console.WriteLine($"Reset link: https://yourdomain.com/reset-password?token={resetToken}");

        return ApiResponse<object>.SuccessResponse(null, "If an account exists with this email, you will receive reset instructions.");
    }

    public async Task<ApiResponse<object>> ResetPasswordAsync(ResetPasswordDto dto)
    {
        // Find valid reset token
        var resetToken = await _passwordResetRepository.GetByTokenAsync(dto.Token);

        if (resetToken == null)
        {
            return ApiResponse<object>.FailureResponse("Invalid or expired reset token");
        }

        // Get user
        var user = await _userRepository.GetByIdAsync(resetToken.UserId);
        if (user == null)
        {
            return ApiResponse<object>.FailureResponse("User not found");
        }

        // Update password
        user.PasswordHash = HashPassword(dto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        // Mark token as used
        resetToken.IsUsed = true;
        resetToken.UsedAt = DateTime.UtcNow;
        await _passwordResetRepository.UpdateAsync(resetToken);

        return ApiResponse<object>.SuccessResponse(null, "Password reset successfully");
    }

    public async Task<ApiResponse<object>> LogoutAsync(LogoutDto dto)
    {
        // Revoke the refresh token
        await _userRepository.RevokeRefreshTokenAsync(dto.RefreshToken);

        return ApiResponse<object>.SuccessResponse(null, "Logged out successfully");
    }

    public (string token, DateTime expiresAt) GenerateAccessToken(User user)
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

    private string GenerateSecureToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
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
