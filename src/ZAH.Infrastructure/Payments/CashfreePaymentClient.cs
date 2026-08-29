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
        var baseUrl = (_options.ApiBaseUrl ?? "https://api.cashfree.com/pg").TrimEnd('/') + "/";
        _httpClient.BaseAddress = new Uri(baseUrl);
    }

    private HttpRequestMessage CreateRequestMessage(HttpMethod method, string relativeUrl, object? content = null, string? idempotencyKey = null)
    {
        EnsureConfigured();
        var message = new HttpRequestMessage(method, relativeUrl);
        if (content != null)
        {
            message.Content = JsonContent.Create(content);
        }
        var clientId = CleanKey(_options.ClientId);
        var clientSecret = CleanKey(_options.ClientSecret);
        var apiVersion = string.IsNullOrWhiteSpace(_options.ApiVersion) ? "2023-08-01" : _options.ApiVersion.Trim();

        message.Headers.TryAddWithoutValidation("x-client-id", clientId);
        message.Headers.TryAddWithoutValidation("x-client-secret", clientSecret);
        message.Headers.TryAddWithoutValidation("x-api-version", apiVersion);
        message.Headers.TryAddWithoutValidation("accept", "application/json");

        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            message.Headers.TryAddWithoutValidation("x-idempotency-key", idempotencyKey);
        }

        return message;
    }

    private static string CleanKey(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        return new string(value.Where(c => char.IsLetterOrDigit(c) || c == '_' || c == '-').ToArray());
    }

    public async Task<CashfreeOrderResult> CreateOrderAsync(string orderId, decimal amount, string name, string email, string? phone, string returnUrl, string webhookUrl, string idempotencyKey, CancellationToken ct)
    {
        var cleanPhone = CleanPhoneForCashfree(phone);
        var orderMeta = new Dictionary<string, string> { ["return_url"] = returnUrl };
        if (!string.IsNullOrWhiteSpace(webhookUrl)) orderMeta["notify_url"] = webhookUrl;
        var requestBody = new
        {
            order_id = orderId,
            order_amount = amount,
            order_currency = "INR",
            customer_details = new { customer_id = orderId, customer_name = name, customer_email = email, customer_phone = cleanPhone },
            order_meta = orderMeta
        };

        using var message = CreateRequestMessage(HttpMethod.Post, "orders", requestBody, idempotencyKey);
        using var response = await _httpClient.SendAsync(message, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            var cid = CleanKey(_options.ClientId);
            var maskedCid = cid.Length > 8 ? $"{cid[..4]}...{cid[^4..]}" : cid;
            var sec = CleanKey(_options.ClientSecret);
            var maskedSec = sec.Length > 8 ? $"{sec[..4]}...{sec[^4..]}" : sec;

            throw new InvalidOperationException($"Cashfree Gateway Error ({response.StatusCode}): {body} (Target: {_httpClient.BaseAddress}, AppID: {maskedCid} [Len {cid.Length}], Secret: {maskedSec} [Len {sec.Length}])");
        }
        return ParseOrder(body);
    }

    private static string CleanPhoneForCashfree(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return "9999999999";
        var digitsOnly = new string(phone.Where(char.IsDigit).ToArray());
        if (digitsOnly.Length == 12 && digitsOnly.StartsWith("91")) return digitsOnly.Substring(2);
        if (digitsOnly.Length == 11 && digitsOnly.StartsWith("0")) return digitsOnly.Substring(1);
        if (digitsOnly.Length == 10) return digitsOnly;
        return digitsOnly.Length >= 10 ? digitsOnly.Substring(digitsOnly.Length - 10) : "9999999999";
    }

    public async Task<CashfreeOrderResult> GetOrderAsync(string orderId, CancellationToken ct)
    {
        using var message = CreateRequestMessage(HttpMethod.Get, $"orders/{Uri.EscapeDataString(orderId)}");
        using var response = await _httpClient.SendAsync(message, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Cashfree GetOrder Error ({response.StatusCode}): {body}");
        }
        return ParseOrder(body);
    }

    public async Task<IReadOnlyList<CashfreePaymentResult>> GetPaymentsAsync(string orderId, CancellationToken ct)
    {
        using var message = CreateRequestMessage(HttpMethod.Get, $"orders/{Uri.EscapeDataString(orderId)}/payments");
        using var response = await _httpClient.SendAsync(message, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Cashfree GetPayments Error ({response.StatusCode}): {body}");
        }
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
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes((_options.ClientSecret ?? string.Empty).Trim()));
        var expected = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload)));
        return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(expected), Encoding.UTF8.GetBytes(signature));
    }

    public string GetReturnUrl(string orderId) =>
        $"{_options.ReturnUrl.TrimEnd('/') }?order_id={Uri.EscapeDataString(orderId)}";

    public string GetWebhookUrl() => _options.WebhookUrl;

    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(_options.ClientId) || string.IsNullOrWhiteSpace(_options.ClientSecret))
            throw new InvalidOperationException($"Cashfree credentials missing in server config (Env: {_options.Environment}, BaseUrl: {_options.ApiBaseUrl})");
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