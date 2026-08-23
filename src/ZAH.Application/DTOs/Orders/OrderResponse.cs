using ZAH.Domain.Entities;
using ZAH.Domain.Enums;

namespace ZAH.Application.DTOs.Orders;

public class OrderResponse
{
    public string Id { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public Address ShippingAddress { get; set; } = new();
    public PaymentMethod PaymentMethod { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal Tax { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public string? TrackingNumber { get; set; }
    public List<OrderTimeline> Timeline { get; set; } = new();
}
