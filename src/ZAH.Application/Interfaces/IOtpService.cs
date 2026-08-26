using ZAH.Application.DTOs;
using ZAH.Shared.Responses;

namespace ZAH.Application.Interfaces;

public interface IOtpService
{
    Task<ApiResponse<object>> SendOtpAsync(SendOtpDto dto);
    Task<ApiResponse<AuthResponseDto>> VerifyOtpAsync(VerifyOtpDto dto);
}
