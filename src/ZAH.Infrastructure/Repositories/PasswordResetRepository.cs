using MongoDB.Driver;
using ZAH.Domain.Entities;
using ZAH.Domain.Interfaces;
using ZAH.Infrastructure.MongoDB;

namespace ZAH.Infrastructure.Repositories;

public class PasswordResetRepository : IPasswordResetRepository
{
    private readonly IMongoCollection<PasswordResetToken> _tokens;

    public PasswordResetRepository(MongoDbContext context)
    {
        _tokens = context.PasswordResetTokens;
        
        // Create indexes
        var tokenIndex = Builders<PasswordResetToken>.IndexKeys.Ascending(t => t.Token);
        var userIdIndex = Builders<PasswordResetToken>.IndexKeys.Ascending(t => t.UserId);
        var expiresAtIndex = Builders<PasswordResetToken>.IndexKeys.Ascending(t => t.ExpiresAt);
        
        _tokens.Indexes.CreateOne(new CreateIndexModel<PasswordResetToken>(tokenIndex));
        _tokens.Indexes.CreateOne(new CreateIndexModel<PasswordResetToken>(userIdIndex));
        _tokens.Indexes.CreateOne(new CreateIndexModel<PasswordResetToken>(expiresAtIndex));
    }

    public async Task<PasswordResetToken> CreateAsync(PasswordResetToken token)
    {
        await _tokens.InsertOneAsync(token);
        return token;
    }

    public async Task<PasswordResetToken?> GetByTokenAsync(string token)
    {
        return await _tokens
            .Find(t => t.Token == token && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow)
            .FirstOrDefaultAsync();
    }

    public async Task<PasswordResetToken> UpdateAsync(PasswordResetToken token)
    {
        token.UpdatedAt = DateTime.UtcNow;
        await _tokens.ReplaceOneAsync(t => t.Id == token.Id, token);
        return token;
    }

    public async Task DeleteExpiredTokensAsync()
    {
        var cutoffTime = DateTime.UtcNow.AddDays(-7);
        await _tokens.DeleteManyAsync(t => t.CreatedAt < cutoffTime);
    }
}
