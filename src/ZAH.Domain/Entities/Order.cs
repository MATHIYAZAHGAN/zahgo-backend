using MongoDB.Bson.Serialization.Attributes;
using ZAH.Domain.Enums;

namespace ZAH.Domain.Entities;

[BsonIgnoreExtraElements]
public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty; // ZAH-YYYYMMDD-NNNNNN
    public string UserId { get; set; } = string.Empty;
    public List<OrderItem> Items { get; set; } = new();
    public Address ShippingAddress { get; set; } = new();
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? AppliedCouponCode { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal Tax { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public string? TrackingNumber { get; set; }
    public List<OrderTimeline> Timeline { get; set; } = new();
    public string? CancellationReason { get; set; }
    public string? ReturnReason { get; set; }
    public string? PaymentTransactionId { get; set; }
    public string? PaymentGatewayOrderId { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? IdempotencyKey { get; set; }
}

public class OrderItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string ProductSlug { get; set; } = string.Empty;
    public string? ProductImage { get; set; }
    public string? SelectedColor { get; set; }
    public string? SelectedSize { get; set; }
    public string? Sku { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalPrice { get; set; }
}

public class OrderTimeline
{
    public OrderStatus Status { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public bool Completed { get; set; }
}
