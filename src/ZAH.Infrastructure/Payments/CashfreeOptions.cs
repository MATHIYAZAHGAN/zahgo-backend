namespace ZAH.Infrastructure.Payments;

public class CashfreeOptions
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string Environment { get; set; } = "sandbox";
    public string ApiBaseUrl { get; set; } = "https://sandbox.cashfree.com/pg";
    public string ReturnUrl { get; set; } = "http://localhost:4200/payment/callback";
    public string WebhookUrl { get; set; } = "";
    public string ApiVersion { get; set; } = "2026-01-01";
}