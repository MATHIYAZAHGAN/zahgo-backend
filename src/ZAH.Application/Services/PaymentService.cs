using System.Text.Json;
using ZAH.Application.DTOs.Payments;
using ZAH.Application.Interfaces;
using ZAH.Domain.Entities;
using ZAH.Domain.Enums;

namespace ZAH.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IAuthService _authService;
    private readonly IProductRepository _products;
    private readonly ICouponRepository _coupons;
    private readonly IOrderRepository _orders;
    private readonly ICashfreePaymentClient _cashfree;

    public PaymentService(IAuthService authService, IProductRepository products, ICouponRepository coupons, IOrderRepository orders, ICashfreePaymentClient cashfree)
    {
        _authService = authService;
        _products = products;
        _coupons = coupons;
        _orders = orders;
        _cashfree = cashfree;
    }

    public async Task<PaymentOrderResponse> CreateOrderAsync(string userId, CreatePaymentOrderRequest request, CancellationToken ct)
    {
        var idempotencyKey = string.IsNullOrWhiteSpace(request.IdempotencyKey) ? Guid.NewGuid().ToString() : request.IdempotencyKey.Trim();
        var existingOrder = await _orders.GetByIdempotencyKeyAsync(userId, idempotencyKey, ct);
        if (existingOrder?.PaymentSessionId is not null)
            return new PaymentOrderResponse { OrderId = existingOrder.Id!, OrderNumber = existingOrder.OrderNumber, PaymentSessionId = existingOrder.PaymentSessionId, Amount = existingOrder.TotalAmount, Currency = "INR" };
        var user = await _authService.GetUserEntityByIdAsync(userId) ?? throw new InvalidOperationException("User not found");
        var address = user.Addresses.FirstOrDefault(item => item.Id == request.AddressId)
            ?? user.Addresses.FirstOrDefault()
            ?? new Address
            {
                FullName = string.IsNullOrWhiteSpace(user.Name) ? "Valued Customer" : user.Name,
                Phone = !string.IsNullOrWhiteSpace(user.Phone) ? user.Phone : "9876543210",
                StreetAddress = "Standard Shipping Address",
                City = "Mumbai",
                State = "Maharashtra",
                Pincode = "400001",
                Type = AddressType.Home,
                IsDefault = true
            };
        var requestedItems = request.Items.GroupBy(item => item.ProductId).Select(group => new PaymentCartItemRequest
        {
            ProductId = group.Key,
            Quantity = group.Sum(item => item.Quantity),
            SelectedColor = group.First().SelectedColor,
            SelectedSize = group.First().SelectedSize
        }).ToList();
        var orderItems = new List<OrderItem>();

        foreach (var item in requestedItems)
        {
            var product = await _products.GetByIdAsync(item.ProductId, ct) ?? throw new InvalidOperationException("One or more products are unavailable");
            if (!product.InStock || product.StockCount < item.Quantity) throw new InvalidOperationException($"{product.Name} is no longer available in the requested quantity");
            var variant = product.Variants.FirstOrDefault(candidate => candidate.ColorName == item.SelectedColor && candidate.Size == item.SelectedSize);
            if (variant != null && variant.Stock < item.Quantity) throw new InvalidOperationException($"{product.Name} variant is no longer available");
            var unitPrice = product.Price + (variant?.PriceModifier ?? 0);
            orderItems.Add(new OrderItem { ProductId = product.Id, ProductName = product.Name, ProductSlug = product.Slug, ProductImage = product.Images.FirstOrDefault(), SelectedColor = item.SelectedColor, SelectedSize = item.SelectedSize, Sku = variant?.Sku, Quantity = item.Quantity, UnitPrice = unitPrice, TotalPrice = unitPrice * item.Quantity });
        }

        var subtotal = orderItems.Sum(item => item.TotalPrice);
        var coupon = string.IsNullOrWhiteSpace(request.CouponCode) ? null : await _coupons.GetByCodeAsync(request.CouponCode.Trim().ToUpperInvariant(), ct);
        var discount = coupon != null && coupon.ExpiryDate > DateTime.UtcNow && subtotal >= coupon.MinOrderAmount
            ? Math.Min(subtotal * coupon.DiscountPercentage / 100m, coupon.MaxDiscount ?? decimal.MaxValue)
            : 0m;
        var shipping = subtotal == 0m ? 0m : 1m;
        var tax = decimal.Round((subtotal - discount) * 0.05m, 0, MidpointRounding.AwayFromZero);
        var total = decimal.Round(Math.Max(0, subtotal - discount + shipping + tax), 2);
        var orderNumber = $"ZAH-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..42];
        var order = new Order
        {
            OrderNumber = orderNumber,
            UserId = userId,
            Items = orderItems,
            ShippingAddress = address,
            PaymentMethod = PaymentMethod.Card,
            PaymentStatus = PaymentStatus.Pending,
            Status = OrderStatus.Pending,
            Subtotal = subtotal,
            DiscountAmount = discount,
            AppliedCouponCode = coupon?.Code,
            ShippingFee = shipping,
            Tax = tax,
            TotalAmount = total,
            PaymentProvider = "CASHFREE",
            IdempotencyKey = idempotencyKey,
            Timeline = new() { new OrderTimeline { Status = OrderStatus.Pending, Title = "Payment Pending", Description = "Waiting for payment verification", Timestamp = DateTime.UtcNow, Completed = true } }
        };
        await _orders.CreateAsync(order, ct);

        var cashfree = await _cashfree.CreateOrderAsync(orderNumber, total, user.Name, user.Email, user.Phone, _cashfree.GetReturnUrl(orderNumber), _cashfree.GetWebhookUrl(), order.IdempotencyKey, ct);
        order.PaymentGatewayOrderId = cashfree.OrderId;
        order.PaymentSessionId = cashfree.PaymentSessionId;
        order.PaymentAttempts.Add(new PaymentAttempt { Amount = total, Currency = "INR" });
        await _orders.UpdateAsync(order, ct);
        return new PaymentOrderResponse { OrderId = order.Id!, OrderNumber = order.OrderNumber, PaymentSessionId = order.PaymentSessionId, Amount = total, Currency = "INR" };
    }

    public async Task<PaymentStatusResponse> GetStatusAsync(string userId, string orderId, CancellationToken ct)
    {
        var order = await _orders.GetByIdAsync(orderId, ct) ?? await _orders.GetByOrderNumberAsync(orderId, ct);
        if (order == null || order.UserId != userId) throw new UnauthorizedAccessException("Order not found");
        if (!string.IsNullOrWhiteSpace(order.PaymentGatewayOrderId)) await ReconcileAsync(order, null, ct);
        return ToStatus(order);
    }

    public async Task<bool> ProcessWebhookAsync(string rawBody, string signature, string timestamp, CancellationToken ct)
    {
        if (!_cashfree.VerifyWebhook(rawBody, signature, timestamp)) return false;
        using var document = JsonDocument.Parse(rawBody);
        var data = document.RootElement.GetProperty("data");
        var gatewayOrderId = data.GetProperty("order").GetProperty("order_id").GetString();
        if (string.IsNullOrWhiteSpace(gatewayOrderId)) return false;
        var order = await _orders.GetByCashfreeOrderIdAsync(gatewayOrderId, ct);
        if (order == null) return false;
        var eventId = data.TryGetProperty("payment", out var payment) && payment.TryGetProperty("cf_payment_id", out var paymentId)
            ? paymentId.GetString() ?? $"{gatewayOrderId}:{timestamp}"
            : $"{gatewayOrderId}:{timestamp}";
        if (order.ProcessedWebhookEventIds.Contains(eventId)) return true;
        await ReconcileAsync(order, eventId, ct);
        return true;
    }

    private async Task ReconcileAsync(Order order, string? eventId, CancellationToken ct)
    {
        if (order.PaymentStatus == PaymentStatus.Paid) return;
        try
        {
            var gatewayOrder = await _cashfree.GetOrderAsync(order.PaymentGatewayOrderId!, ct);
            var payments = await _cashfree.GetPaymentsAsync(order.PaymentGatewayOrderId!, ct);
            var successfulPayment = payments.FirstOrDefault(payment =>
                payment.Status.Equals("SUCCESS", StringComparison.OrdinalIgnoreCase));

            bool isPaid = string.Equals(gatewayOrder.Status, "PAID", StringComparison.OrdinalIgnoreCase)
                       || successfulPayment != null;

            if (!isPaid)
            {
                order.PaymentVerificationStatus = "MISMATCH_OR_PENDING";
                await _orders.TryUpdatePaymentAsync(order, eventId, ct);
                return;
            }

            order.PaymentStatus = PaymentStatus.Paid;
            order.Status = OrderStatus.Confirmed;
            order.PaidAt = DateTime.UtcNow;
            order.PaymentTransactionId = successfulPayment?.PaymentId ?? order.PaymentGatewayOrderId;
            order.PaymentVerificationStatus = "VERIFIED";
            if (eventId != null) order.ProcessedWebhookEventIds.Add(eventId);
            var attempt = order.PaymentAttempts.LastOrDefault();
            if (attempt != null)
            {
                attempt.ProviderPaymentId = order.PaymentTransactionId;
                attempt.Status = "SUCCESS";
                attempt.CompletedAt = DateTime.UtcNow;
            }
            await _orders.TryUpdatePaymentAsync(order, eventId, ct);
        }
        catch
        {
            // Allow transient gateway inspection errors to degrade gracefully
        }
    }

    private static PaymentStatusResponse ToStatus(Order order) => new()
    {
        OrderId = order.Id!, OrderNumber = order.OrderNumber, Status = order.PaymentStatus.ToString().ToUpperInvariant(), Amount = order.TotalAmount, Currency = "INR"
    };
}