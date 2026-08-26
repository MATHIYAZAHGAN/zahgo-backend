using MongoDB.Driver;
using ZAH.Domain.Entities;
using ZAH.Domain.Interfaces;
using ZAH.Infrastructure.MongoDB;

namespace ZAH.Infrastructure.Repositories;

public class OtpRepository : IOtpRepository
{
    private readonly IMongoCollection<OtpVerification> _otps;

    public OtpRepository(MongoDbContext context)
    {
        _otps = context.OtpVerifications;
        
        // Create indexes for efficient querying
        CreateIndexes();
    }

    private void CreateIndexes()
    {
        var phoneIndex = Builders<OtpVerification>.IndexKeys.Ascending(o => o.Phone);
        var createdAtIndex = Builders<OtpVerification>.IndexKeys.Descending(o => o.CreatedAt);
        var expiresAtIndex = Builders<OtpVerification>.IndexKeys.Ascending(o => o.ExpiresAt);
        
        _otps.Indexes.CreateOne(new CreateIndexModel<OtpVerification>(phoneIndex));
        _otps.Indexes.CreateOne(new CreateIndexModel<OtpVerification>(createdAtIndex));
        _otps.Indexes.CreateOne(new CreateIndexModel<OtpVerification>(expiresAtIndex));
    }

    public async Task<OtpVerification> CreateAsync(OtpVerification otp)
    {
        await _otps.InsertOneAsync(otp);
        return otp;
    }

    public async Task<OtpVerification?> GetLatestOtpByPhoneAsync(string phone)
    {
        return await _otps
            .Find(o => o.Phone == phone && !o.IsUsed)
            .SortByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<List<OtpVerification>> GetRecentOtpsByPhoneAsync(string phone, TimeSpan timeSpan)
    {
        var cutoffTime = DateTime.UtcNow - timeSpan;
        return await _otps
            .Find(o => o.Phone == phone && o.CreatedAt >= cutoffTime)
            .ToListAsync();
    }

    public async Task<OtpVerification> UpdateAsync(OtpVerification otp)
    {
        otp.UpdatedAt = DateTime.UtcNow;
        await _otps.ReplaceOneAsync(o => o.Id == otp.Id, otp);
        return otp;
    }

    public async Task DeleteExpiredOtpsAsync()
    {
        var cutoffTime = DateTime.UtcNow.AddDays(-7); // Delete OTPs older than 7 days
        await _otps.DeleteManyAsync(o => o.CreatedAt < cutoffTime);
    }
}
