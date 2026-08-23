using ZAH.Domain.Enums;

namespace ZAH.Application.DTOs.Orders;

public class CreateOrderRequest
{
    public string AddressId { get; set; } = string.Empty;
    public PaymentMethod PaymentMethod { get; set; }
    public string? CouponCode { get; set; }
    public string? IdempotencyKey { get; set; }
}
