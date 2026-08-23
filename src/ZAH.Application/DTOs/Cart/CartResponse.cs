namespace ZAH.Application.DTOs.Cart;

public class CartResponse
{
    public List<CartItemResponse> Items { get; set; } = new();
    public CartSummaryResponse Summary { get; set; } = new();
}

public class CartItemResponse
{
    public string Id { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string ProductSlug { get; set; } = string.Empty;
    public string? ProductImage { get; set; }
    public string? SelectedColor { get; set; }
    public string? SelectedSize { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public bool InStock { get; set; }
    public int AvailableStock { get; set; }
}

public class CartSummaryResponse
{
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? AppliedCouponCode { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal EstimatedTax { get; set; }
    public decimal Total { get; set; }
    public decimal FreeShippingThreshold { get; set; }
    public decimal AmountForFreeShipping { get; set; }
}
