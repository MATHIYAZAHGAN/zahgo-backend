using MongoDB.Bson.Serialization.Attributes;

namespace ZAH.Domain.Entities;

[BsonIgnoreExtraElements]
public class Cart : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public List<CartItem> Items { get; set; } = new();
    public DateTime? ExpiresAt { get; set; }
}

public class CartItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ProductId { get; set; } = string.Empty;
    public string? SelectedColorName { get; set; }
    public string? SelectedColorHex { get; set; }
    public string? SelectedSize { get; set; }
    public int Quantity { get; set; } = 1;
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
