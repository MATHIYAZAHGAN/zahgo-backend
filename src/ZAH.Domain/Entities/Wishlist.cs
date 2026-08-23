using MongoDB.Bson.Serialization.Attributes;

namespace ZAH.Domain.Entities;

[BsonIgnoreExtraElements]
public class Wishlist : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public List<WishlistItem> Items { get; set; } = new();
}

public class WishlistItem
{
    public string ProductId { get; set; } = string.Empty;
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
