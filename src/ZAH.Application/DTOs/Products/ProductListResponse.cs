using ZAH.Domain.Entities;

namespace ZAH.Application.DTOs.Products;

public class ProductListResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal OriginalPrice { get; set; }
    public int DiscountPercentage { get; set; }
    public double Rating { get; set; }
    public int ReviewCount { get; set; }
    public List<string> Images { get; set; } = new();
    public string ShortDescription { get; set; } = string.Empty;
    public bool InStock { get; set; }
    public bool IsNew { get; set; }
    public bool IsBestSeller { get; set; }
    public bool IsTrending { get; set; }
    public bool IsFlashSale { get; set; }
    public List<ProductColor> AvailableColors { get; set; } = new();
    public List<string> AvailableSizes { get; set; } = new();
}
