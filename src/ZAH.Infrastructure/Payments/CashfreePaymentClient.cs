using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ZAH.Application.Interfaces;

namespace ZAH.Infrastructure.Payments;

public class CashfreePaymentClient : ICashfreePaymentClient
{
    private readonly HttpClient _httpClient;
    private readonly CashfreeOptions _options;

    public CashfreePaymentClient(HttpClient httpClient, IOptions<CashfreeOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _httpClient.BaseAddress = new Uri(_options.ApiBaseUrl.TrimEnd('/') + "/");
        _httpClient.DefaultRequestHeaders.Add("x-client-id", _options.ClientId);
        _httpClient.DefaultRequestHeaders.Add("x-client-secret", _options.ClientSecret);
        _httpClient.DefaultRequestHeaders.Add("x-api-version", _options.ApiVersion);
        _httpClient.DefaultRequestHeaders.Add("accept", "application/json");
    }

    public async Task<CashfreeOrderResult> CreateOrderAsync(string orderId, decimal amount, string name, string email, string? phone, string returnUrl, string webhookUrl, string idempotencyKey, CancellationToken ct)
    {
        EnsureConfigured();
        var orderMeta = new Dictionary<string, string> { ["return_url"] = returnUrl };
        if (!string.IsNullOrWhiteSpace(webhookUrl)) orderMeta["notify_url"] = webhookUrl;
        var request = new
        {
            order_id = orderId,
            order_amount = amount,
            order_currency = "INR",
            customer_details = new { customer_id = orderId, customer_name = name, customer_email = email, customer_phone = phone ?? "9999999999" },
            order_meta = orderMeta
        };
        using var message = new HttpRequestMessage(HttpMethod.Post, "orders") { Content = JsonContent.Create(request) };
        message.Headers.Add("x-idempotency-key", idempotencyKey);
        using var response = await _httpClient.SendAsync(message, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        response.EnsureSuccessStatusCode();
        return ParseOrder(body);
    }

    public async Task<CashfreeOrderResult> GetOrderAsync(string orderId, CancellationToken ct)
    {
        EnsureConfigured();
        using var response = await _httpClient.GetAsync($"orders/{Uri.EscapeDataString(orderId)}", ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        response.EnsureSuccessStatusCode();
        return ParseOrder(body);
    }

    public async Task<IReadOnlyList<CashfreePaymentResult>> GetPaymentsAsync(string orderId, CancellationToken ct)
    {
        EnsureConfigured();
        using var response = await _httpClient.GetAsync($"orders/{Uri.EscapeDataString(orderId)}/payments", ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        response.EnsureSuccessStatusCode();
        using var document = JsonDocument.Parse(body);
        return document.RootElement.EnumerateArray().Select(payment => new CashfreePaymentResult(
            payment.GetProperty("cf_payment_id").GetString() ?? "",
            payment.GetProperty("order_id").GetString() ?? "",
            payment.GetProperty("payment_amount").GetDecimal(),
            payment.GetProperty("payment_currency").GetString() ?? "",
            payment.GetProperty("payment_status").GetString() ?? "")).ToList();
    }

    public bool VerifyWebhook(string rawBody, string signature, string timestamp)
    {
        if (string.IsNullOrWhiteSpace(signature) || string.IsNullOrWhiteSpace(timestamp)) return false;
        if (!long.TryParse(timestamp, out var milliseconds) || Math.Abs((DateTimeOffset.UtcNow - DateTimeOffset.FromUnixTimeMilliseconds(milliseconds)).TotalMinutes) > 5) return false;
        var payload = timestamp + rawBody;
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.ClientSecret));
        var expected = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)));
        return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(expected), Encoding.UTF8.GetBytes(signature));
    }

    public string GetReturnUrl(string orderId) =>
        $"{_options.ReturnUrl.TrimEnd('/') }?order_id={Uri.EscapeDataString(orderId)}";

    public string GetWebhookUrl() => _options.WebhookUrl;

    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(_options.ClientId) || string.IsNullOrWhiteSpace(_options.ClientSecret))
            throw new InvalidOperationException("Cashfree sandbox credentials are not configured on the server");
    }

    private static CashfreeOrderResult ParseOrder(string body)
    {
        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;
        return new CashfreeOrderResult(
            root.GetProperty("order_id").GetString() ?? "",
            root.GetProperty("payment_session_id").GetString() ?? "",
            root.GetProperty("order_amount").GetDecimal(),
            root.GetProperty("order_currency").GetString() ?? "",
            root.GetProperty("order_status").GetString() ?? "");
    }
}