using MongoDB.Bson.Serialization.Attributes;
using ZAH.Domain.Enums;

namespace ZAH.Domain.Entities;

[BsonIgnoreExtraElements]
public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal OriginalPrice { get; set; }
    public int DiscountPercentage { get; set; }
    public double Rating { get; set; } = 0;
    public int ReviewCount { get; set; } = 0;
    public List<string> Images { get; set; } = new();
    public string Description { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public bool InStock { get; set; } = true;
    public int StockCount { get; set; } = 0;
    public int ReservedStock { get; set; } = 0; // For pending orders
    public int LowStockThreshold { get; set; } = 5;
    public ProductStatus Status { get; set; } = ProductStatus.Active;
    public bool IsNew { get; set; } = false;
    public bool IsBestSeller { get; set; } = false;
    public bool IsTrending { get; set; } = false;
    public bool IsFlashSale { get; set; } = false;
    public List<string> Tags { get; set; } = new();
    public List<ProductColor> AvailableColors { get; set; } = new();
    public List<string> AvailableSizes { get; set; } = new();
    public List<ProductVariant> Variants { get; set; } = new();
    public List<ProductSpecification> Specifications { get; set; } = new();
    public List<ProductReview> Reviews { get; set; } = new();
    public Dictionary<string, string> Attributes { get; set; } = new();
    public int ViewCount { get; set; } = 0;
    public int PurchaseCount { get; set; } = 0;
}

public class ProductColor
{
    public string Name { get; set; } = string.Empty;
    public string Hex { get; set; } = string.Empty;
}

public class ProductVariant
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ColorName { get; set; } = string.Empty;
    public string ColorHex { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int Stock { get; set; } = 0;
    public decimal PriceModifier { get; set; } = 0;
    public string? Image { get; set; }
}

public class ProductSpecification
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class ProductReview
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public double Rating { get; set; }
    public string Date { get; set; } = DateTime.UtcNow.ToString("dd MMM yyyy");
    public string Comment { get; set; } = string.Empty;
    public bool VerifiedPurchase { get; set; } = false;
    public int Likes { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
