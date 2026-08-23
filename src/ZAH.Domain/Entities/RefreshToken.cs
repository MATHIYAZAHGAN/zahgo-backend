using MongoDB.Bson.Serialization.Attributes;

namespace ZAH.Domain.Entities;

[BsonIgnoreExtraElements]
public class RefreshToken : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    public DateTime? RevokedAt { get; set; }
    public string? DeviceInfo { get; set; }
    public string? IpAddress { get; set; }
}
