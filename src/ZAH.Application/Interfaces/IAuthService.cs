using ZAH.Application.DTOs;
using ZAH.Domain.Entities;
using ZAH.Shared.Responses;

namespace ZAH.Application.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto dto);
    Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto dto);
    Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(string refreshToken);
    Task<ApiResponse<object>> GetUserByIdAsync(string userId);
    Task<User?> GetUserEntityByIdAsync(string userId);
    Task<User> UpdateUserAsync(User user);
    Task<ApiResponse<object>> ForgotPasswordAsync(ForgotPasswordDto dto);
    Task<ApiResponse<object>> ResetPasswordAsync(ResetPasswordDto dto);
    Task<ApiResponse<object>> LogoutAsync(LogoutDto dto);
}
