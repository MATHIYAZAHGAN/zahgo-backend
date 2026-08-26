using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using ZAH.Application.DTOs;
using ZAH.Application.Interfaces;
using ZAH.Domain.Entities;
using ZAH.Domain.Enums;
using ZAH.Domain.Interfaces;
using ZAH.Shared.Responses;

namespace ZAH.Application.Services;

public class OtpService : IOtpService
{
    private readonly IOtpRepository _otpRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<OtpService> _logger;
    private readonly IAuthService _authService;
    private readonly ISmsService _smsService;

    public OtpService(
        IOtpRepository otpRepository,
        IUserRepository userRepository,
        ILogger<OtpService> logger,
        IAuthService authService,
        ISmsService smsService)
    {
        _otpRepository = otpRepository;
        _userRepository = userRepository;
        _logger = logger;
        _authService = authService;
        _smsService = smsService;
    }

    public async Task<ApiResponse<object>> SendOtpAsync(SendOtpDto dto)
    {
        try
        {
            // Rate limiting - check recent OTP requests for this phone
            var recentOtps = await _otpRepository.GetRecentOtpsByPhoneAsync(dto.Phone, TimeSpan.FromMinutes(5));
            if (recentOtps.Count >= 3)
            {
                _logger.LogWarning("Too many OTP requests for phone: {Phone}", dto.Phone);
                return ApiResponse<object>.FailureResponse("Too many OTP requests. Please try again later.");
            }

            // Check for active OTP cooldown (last OTP sent within 60 seconds)
            var lastOtp = await _otpRepository.GetLatestOtpByPhoneAsync(dto.Phone);
            if (lastOtp != null && lastOtp.CreatedAt > DateTime.UtcNow.AddSeconds(-60))
            {
                var waitTime = 60 - (int)(DateTime.UtcNow - lastOtp.CreatedAt).TotalSeconds;
                return ApiResponse<object>.FailureResponse($"Please wait {waitTime} seconds before requesting a new OTP.");
            }

            // Generate 6-digit OTP
            var otp = GenerateOtp();
            _logger.LogInformation("Generated OTP for phone: {Phone}", dto.Phone);

            // Hash the OTP before storing
            var otpHash = HashOtp(otp);

            // Create OTP verification record
            var otpVerification = new OtpVerification
            {
                Phone = dto.Phone,
                OtpHash = otpHash,
                Purpose = "LOGIN",
                ExpiresAt = DateTime.UtcNow.AddMinutes(10), // 10 minute expiration
                AttemptCount = 0,
                MaxAttempts = 5,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _otpRepository.CreateAsync(otpVerification);

            // Send OTP via SMS service
            var smsMessage = $"Your ZAHGO verification code is: {otp}. Valid for 10 minutes. Do not share this code.";
            var smsSent = await _smsService.SendAsync(dto.Phone, smsMessage);

            if (!smsSent)
            {
                _logger.LogError("Failed to send SMS to {Phone}", dto.Phone);
                // Still return success to user for security (don't reveal SMS failures)
                // But log the OTP for development/debugging
                _logger.LogInformation("OTP for {Phone}: {Otp} (SMS FAILED - LOGGED FOR DEBUGGING)", dto.Phone, otp);
            }

            // In production, replace above with actual SMS sending:
            // await _smsService.SendAsync(dto.Phone, $"Your ZAHGO verification code is: {otp}. Valid for 10 minutes.");

            return ApiResponse<object>.SuccessResponse(
                new { expiresAt = otpVerification.ExpiresAt },
                "OTP sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending OTP to phone: {Phone}", dto.Phone);
            return ApiResponse<object>.FailureResponse("Failed to send OTP. Please try again.");
        }
    }

    public async Task<ApiResponse<AuthResponseDto>> VerifyOtpAsync(VerifyOtpDto dto)
    {
        try
        {
            // Get the latest valid OTP for this phone
            var otpVerification = await _otpRepository.GetLatestOtpByPhoneAsync(dto.Phone);

            if (otpVerification == null)
            {
                _logger.LogWarning("No OTP found for phone: {Phone}", dto.Phone);
                return ApiResponse<AuthResponseDto>.FailureResponse("Invalid or expired OTP.");
            }

            // Check if OTP is already used
            if (otpVerification.IsUsed)
            {
                _logger.LogWarning("Attempted to use already-used OTP for phone: {Phone}", dto.Phone);
                return ApiResponse<AuthResponseDto>.FailureResponse("This OTP has already been used.");
            }

            // Check if OTP is expired
            if (otpVerification.ExpiresAt < DateTime.UtcNow)
            {
                _logger.LogWarning("Expired OTP verification attempt for phone: {Phone}", dto.Phone);
                return ApiResponse<AuthResponseDto>.FailureResponse("OTP has expired. Please request a new one.");
            }

            // Check attempt count
            if (otpVerification.AttemptCount >= otpVerification.MaxAttempts)
            {
                _logger.LogWarning("Max OTP verification attempts exceeded for phone: {Phone}", dto.Phone);
                return ApiResponse<AuthResponseDto>.FailureResponse("Too many failed attempts. Please request a new OTP.");
            }

            // Verify OTP
            var isValid = VerifyOtpHash(dto.Otp, otpVerification.OtpHash);

            // Increment attempt count
            otpVerification.AttemptCount++;
            await _otpRepository.UpdateAsync(otpVerification);

            if (!isValid)
            {
                _logger.LogWarning("Invalid OTP provided for phone: {Phone}. Attempt: {Attempt}/{MaxAttempts}",
                    dto.Phone, otpVerification.AttemptCount, otpVerification.MaxAttempts);
                return ApiResponse<AuthResponseDto>.FailureResponse("Invalid OTP. Please try again.");
            }

            // Mark OTP as used
            otpVerification.IsUsed = true;
            otpVerification.UsedAt = DateTime.UtcNow;
            await _otpRepository.UpdateAsync(otpVerification);

            _logger.LogInformation("OTP verified successfully for phone: {Phone}", dto.Phone);

            // Find or create user
            var user = await _userRepository.GetByPhoneAsync(dto.Phone);

            if (user == null)
            {
                // Create new user with phone number
                user = new User
                {
                    Name = $"User {dto.Phone.Substring(Math.Max(0, dto.Phone.Length - 4))}",
                    Email = $"{dto.Phone}@zahgo.temp", // Temporary email
                    Phone = dto.Phone,
                    PasswordHash = string.Empty, // No password for OTP users
                    Role = UserRole.Customer,
                    RewardPoints = 100, // Welcome bonus
                    IsPhoneVerified = true,
                    IsEmailVerified = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _userRepository.CreateAsync(user);
                _logger.LogInformation("Created new user via OTP for phone: {Phone}", dto.Phone);
            }
            else
            {
                // Update phone verification status
                user.IsPhoneVerified = true;
                user.LastLoginAt = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);
            }

            // Generate authentication tokens using existing auth service
            var (accessToken, expiresAt) = ((AuthService)_authService).GenerateAccessToken(user);
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

            return ApiResponse<AuthResponseDto>.SuccessResponse(response, "Verified successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying OTP for phone: {Phone}", dto.Phone);
            return ApiResponse<AuthResponseDto>.FailureResponse("Verification failed. Please try again.");
        }
    }

    private string GenerateOtp()
    {
        // Generate cryptographically secure 6-digit OTP
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[4];
        rng.GetBytes(bytes);
        var num = BitConverter.ToUInt32(bytes, 0);
        return (num % 1000000).ToString("D6");
    }

    private string HashOtp(string otp)
    {
        // Use BCrypt for OTP hashing (same as password hashing)
        return BCrypt.Net.BCrypt.HashPassword(otp);
    }

    private bool VerifyOtpHash(string otp, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(otp, hash);
    }

    private string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
