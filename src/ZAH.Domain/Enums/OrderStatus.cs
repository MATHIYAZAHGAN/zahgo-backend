namespace ZAH.Domain.Enums;

public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Processing = 2,
    Packed = 3,
    Shipped = 4,
    OutForDelivery = 5,
    Delivered = 6,
    Cancelled = 7,
    Returned = 8,
    Refunded = 9
}
