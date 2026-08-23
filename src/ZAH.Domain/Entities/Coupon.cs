using MongoDB.Bson.Serialization.Attributes;

namespace ZAH.Domain.Entities;

[BsonIgnoreExtraElements]
public class Coupon : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DiscountPercentage { get; set; }
    public decimal? MaxDiscount { get; set; }
    public decimal MinOrderAmount { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool IsActive { get; set; } = true;
    public int? UsageLimit { get; set; }
    public int UsedCount { get; set; } = 0;
    public int? PerUserLimit { get; set; }
    public List<string>? ApplicableCategories { get; set; }
    public List<string>? ApplicableProducts { get; set; }
    public bool IsFirstOrderOnly { get; set; } = false;
}
