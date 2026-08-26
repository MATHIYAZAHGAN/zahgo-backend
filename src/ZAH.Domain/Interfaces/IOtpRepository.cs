using ZAH.Domain.Entities;

namespace ZAH.Domain.Interfaces;

public interface IOtpRepository
{
    Task<OtpVerification> CreateAsync(OtpVerification otp);
    Task<OtpVerification?> GetLatestOtpByPhoneAsync(string phone);
    Task<List<OtpVerification>> GetRecentOtpsByPhoneAsync(string phone, TimeSpan timeSpan);
    Task<OtpVerification> UpdateAsync(OtpVerification otp);
    Task DeleteExpiredOtpsAsync();
}
