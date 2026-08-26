using MongoDB.Driver;
using ZAH.Application.Interfaces;
using ZAH.Domain.Entities;
using ZAH.Infrastructure.MongoDB;

namespace ZAH.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly MongoDbContext _context;

    public UserRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(string id)
    {
        return await _context.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.Find(u => u.Email == email).FirstOrDefaultAsync();
    }

    public async Task<User?> GetByPhoneAsync(string phone)
    {
        return await _context.Users.Find(u => u.Phone == phone).FirstOrDefaultAsync();
    }

    public async Task<User> CreateAsync(User user)
    {
        await _context.Users.InsertOneAsync(user);
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        await _context.Users.ReplaceOneAsync(u => u.Id == user.Id, user);
        return user;
    }

    public async Task DeleteAsync(string id)
    {
        await _context.Users.DeleteOneAsync(u => u.Id == id);
    }

    public async Task SaveRefreshTokenAsync(RefreshToken refreshToken)
    {
        await _context.RefreshTokens.InsertOneAsync(refreshToken);
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
    {
        return await _context.RefreshTokens.Find(rt => rt.Token == token).FirstOrDefaultAsync();
    }

    public async Task UpdateRefreshTokenAsync(RefreshToken refreshToken)
    {
        await _context.RefreshTokens.ReplaceOneAsync(rt => rt.Id == refreshToken.Id, refreshToken);
    }

    public async Task RevokeRefreshTokenAsync(string token)
    {
        var refreshToken = await GetRefreshTokenAsync(token);
        if (refreshToken != null)
        {
            refreshToken.IsRevoked = true;
            await UpdateRefreshTokenAsync(refreshToken);
        }
    }
}
