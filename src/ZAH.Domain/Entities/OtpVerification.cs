using MongoDB.Bson.Serialization.Attributes;

namespace ZAH.Domain.Entities;

[BsonIgnoreExtraElements]
public class OtpVerification : BaseEntity
{
    public string Phone { get; set; } = string.Empty;
    public string OtpHash { get; set; } = string.Empty;
    public string Purpose { get; set; } = "LOGIN"; // LOGIN, REGISTRATION, PASSWORD_RESET
    public DateTime ExpiresAt { get; set; }
    public int AttemptCount { get; set; } = 0;
    public bool IsUsed { get; set; } = false;
    public DateTime? UsedAt { get; set; }
    public int MaxAttempts { get; set; } = 5;
}
