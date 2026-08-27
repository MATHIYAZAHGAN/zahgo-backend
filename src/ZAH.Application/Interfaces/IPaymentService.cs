using ZAH.Application.DTOs.Payments;

namespace ZAH.Application.Interfaces;

public interface IPaymentService
{
    Task<PaymentOrderResponse> CreateOrderAsync(string userId, CreatePaymentOrderRequest request, CancellationToken ct);
    Task<PaymentStatusResponse> GetStatusAsync(string userId, string orderId, CancellationToken ct);
    Task<bool> ProcessWebhookAsync(string rawBody, string signature, string timestamp, CancellationToken ct);
}

public interface ICashfreePaymentClient
{
    Task<CashfreeOrderResult> CreateOrderAsync(string orderId, decimal amount, string name, string email, string? phone, string returnUrl, string webhookUrl, string idempotencyKey, CancellationToken ct);
    Task<CashfreeOrderResult> GetOrderAsync(string orderId, CancellationToken ct);
    Task<IReadOnlyList<CashfreePaymentResult>> GetPaymentsAsync(string orderId, CancellationToken ct);
    bool VerifyWebhook(string rawBody, string signature, string timestamp);
    string GetReturnUrl(string orderId);
    string GetWebhookUrl();
}

public record CashfreeOrderResult(string OrderId, string PaymentSessionId, decimal Amount, string Currency, string Status);
public record CashfreePaymentResult(string PaymentId, string OrderId, decimal Amount, string Currency, string Status);