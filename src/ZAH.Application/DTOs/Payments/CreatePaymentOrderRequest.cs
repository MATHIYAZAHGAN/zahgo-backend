using System.ComponentModel.DataAnnotations;

namespace ZAH.Application.DTOs.Payments;

public class CreatePaymentOrderRequest
{
    [Required] public string AddressId { get; set; } = string.Empty;
    [Required, MinLength(1)] public List<PaymentCartItemRequest> Items { get; set; } = new();
    public string? CouponCode { get; set; }
    public string? IdempotencyKey { get; set; }
}

public class PaymentCartItemRequest
{
    [Required] public string ProductId { get; set; } = string.Empty;
    [Range(1, 50)] public int Quantity { get; set; } = 1;
    public string? SelectedColor { get; set; }
    public string? SelectedSize { get; set; }
}