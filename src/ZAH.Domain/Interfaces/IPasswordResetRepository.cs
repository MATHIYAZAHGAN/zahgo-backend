using ZAH.Domain.Entities;

namespace ZAH.Domain.Interfaces;

public interface IPasswordResetRepository
{
    Task<PasswordResetToken> CreateAsync(PasswordResetToken token);
    Task<PasswordResetToken?> GetByTokenAsync(string token);
    Task<PasswordResetToken> UpdateAsync(PasswordResetToken token);
    Task DeleteExpiredTokensAsync();
}
