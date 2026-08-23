using ZAH.Application.DTOs;
using ZAH.Shared.Responses;

namespace ZAH.Application.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto dto);
    Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto dto);
    Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(string refreshToken);
    Task<ApiResponse<object>> GetUserByIdAsync(string userId);
}
