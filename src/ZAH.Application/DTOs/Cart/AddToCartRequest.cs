namespace ZAH.Application.DTOs.Cart;

public class AddToCartRequest
{
    public string ProductId { get; set; } = string.Empty;
    public string? SelectedColorName { get; set; }
    public string? SelectedColorHex { get; set; }
    public string? SelectedSize { get; set; }
    public int Quantity { get; set; } = 1;
}
