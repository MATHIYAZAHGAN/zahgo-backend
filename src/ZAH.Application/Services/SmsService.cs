using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ZAH.Application.Services;

public interface ISmsService
{
    Task<bool> SendAsync(string phoneNumber, string message);
    Task<bool> SendOtpAsync(string phoneNumber, string otpCode, string message = "");
}

public class TwoFactorSmsService : ISmsService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TwoFactorSmsService> _logger;

    public TwoFactorSmsService(HttpClient httpClient, IConfiguration configuration, ILogger<TwoFactorSmsService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SendOtpAsync(string phoneNumber, string otpCode, string message = "")
    {
        var apiKey = GetApiKey();
        var cleanPhone = CleanPhoneNumber(phoneNumber);

        if (string.IsNullOrEmpty(apiKey) || apiKey.StartsWith("${"))
        {
            _logger.LogWarning("2Factor API key not configured. Logging OTP in Development Mode.");
            _logger.LogInformation("2Factor SMS for {Phone}: OTP = {Otp} (DEVELOPMENT MODE - NO REAL SMS SENT)", phoneNumber, otpCode);
            return true;
        }

        try
        {
            // 2Factor Direct OTP API URL: https://2factor.in/API/V1/{api_key}/SMS/{phone_number}/{otp_val}
            var url = $"https://2factor.in/API/V1/{apiKey}/SMS/{cleanPhone}/{otpCode}";
            _logger.LogInformation("Sending OTP to {Phone} via 2Factor.in", cleanPhone);

            var response = await _httpClient.GetAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode && Is2FactorSuccess(responseContent))
            {
                _logger.LogInformation("2Factor SMS OTP sent successfully to {Phone}. Response: {Response}", cleanPhone, responseContent);
                return true;
            }
            else
            {
                _logger.LogError("Failed to send 2Factor OTP to {Phone}. Response: {Response}", cleanPhone, responseContent);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending 2Factor OTP to {Phone}", phoneNumber);
            return false;
        }
    }

    public async Task<bool> SendAsync(string phoneNumber, string message)
    {
        // Try extracting numeric OTP from message if present
        var match = Regex.Match(message, @"\b\d{4,6}\b");
        if (match.Success)
        {
            return await SendOtpAsync(phoneNumber, match.Value, message);
        }

        var apiKey = GetApiKey();
        var cleanPhone = CleanPhoneNumber(phoneNumber);

        if (string.IsNullOrEmpty(apiKey) || apiKey.StartsWith("${"))
        {
            _logger.LogWarning("2Factor API key not configured. Logging SMS in Development Mode.");
            _logger.LogInformation("2Factor SMS for {Phone}: {Message} (DEVELOPMENT MODE - NO REAL SMS SENT)", phoneNumber, message);
            return true;
        }

        try
        {
            // Transactional SMS API: https://2factor.in/API/V1/{api_key}/ADDON_SERVICES/SEND/TSMS
            var url = $"https://2factor.in/API/V1/{apiKey}/ADDON_SERVICES/SEND/TSMS";
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("From", "2FACTOR"),
                new KeyValuePair<string, string>("To", cleanPhone),
                new KeyValuePair<string, string>("Msg", message)
            });

            var response = await _httpClient.PostAsync(url, formContent);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode && Is2FactorSuccess(responseContent))
            {
                _logger.LogInformation("2Factor SMS sent successfully to {Phone}. Response: {Response}", cleanPhone, responseContent);
                return true;
            }
            else
            {
                _logger.LogError("Failed to send 2Factor SMS to {Phone}. Response: {Response}", cleanPhone, responseContent);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending 2Factor SMS to {Phone}", phoneNumber);
            return false;
        }
    }

    private string GetApiKey()
    {
        return _configuration["TwoFactor:ApiKey"]
            ?? _configuration["TWOFACTOR_API_KEY"]
            ?? Environment.GetEnvironmentVariable("TWOFACTOR_API_KEY")
            ?? Environment.GetEnvironmentVariable("TwoFactor__ApiKey")
            ?? Environment.GetEnvironmentVariable("2FACTOR_API_KEY")
            ?? string.Empty;
    }

    private string CleanPhoneNumber(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return string.Empty;
        var digitsOnly = new string(phone.Where(char.IsDigit).ToArray());
        if (digitsOnly.Length == 12 && digitsOnly.StartsWith("91"))
        {
            return digitsOnly.Substring(2);
        }
        if (digitsOnly.Length == 11 && digitsOnly.StartsWith("0"))
        {
            return digitsOnly.Substring(1);
        }
        return digitsOnly;
    }

    private bool Is2FactorSuccess(string jsonResponse)
    {
        try
        {
            using var doc = JsonDocument.Parse(jsonResponse);
            if (doc.RootElement.TryGetProperty("Status", out var statusProp))
            {
                return string.Equals(statusProp.GetString(), "Success", StringComparison.OrdinalIgnoreCase);
            }
        }
        catch { }
        return jsonResponse.Contains("Success", StringComparison.OrdinalIgnoreCase);
    }
}

public class TwilioSmsService : ISmsService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TwilioSmsService> _logger;

    public TwilioSmsService(HttpClient httpClient, IConfiguration configuration, ILogger<TwilioSmsService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public Task<bool> SendOtpAsync(string phoneNumber, string otpCode, string message = "")
    {
        var body = string.IsNullOrEmpty(message) ? $"Your verification code is {otpCode}" : message;
        return SendAsync(phoneNumber, body);
    }

    public async Task<bool> SendAsync(string phoneNumber, string message)
    {
        try
        {
            var accountSid = _configuration["Twilio:AccountSid"];
            var authToken = _configuration["Twilio:AuthToken"];
            var fromNumber = _configuration["Twilio:PhoneNumber"];

            if (string.IsNullOrEmpty(accountSid) || string.IsNullOrEmpty(authToken) || string.IsNullOrEmpty(fromNumber) || accountSid.StartsWith("${"))
            {
                _logger.LogWarning("Twilio configuration not found. SMS will be logged instead of sent.");
                _logger.LogInformation("SMS for {Phone}: {Message} (DEVELOPMENT MODE - NO REAL SMS SENT)", phoneNumber, message);
                return true;
            }

            var twilioUrl = $"https://api.twilio.com/2010-04-01/Accounts/{accountSid}/Messages.json";
            var parameters = new List<KeyValuePair<string, string>>
            {
                new("To", phoneNumber),
                new("From", fromNumber),
                new("Body", message)
            };

            var encodedContent = new FormUrlEncodedContent(parameters);
            var credentials = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{accountSid}:{authToken}"));
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

            var response = await _httpClient.PostAsync(twilioUrl, encodedContent);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("SMS sent successfully to {Phone} via Twilio", phoneNumber);
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to send SMS to {Phone}. Twilio error: {Error}", phoneNumber, errorContent);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending SMS to {Phone} via Twilio", phoneNumber);
            return false;
        }
    }
}

public class Fast2SmsService : ISmsService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<Fast2SmsService> _logger;

    public Fast2SmsService(HttpClient httpClient, IConfiguration configuration, ILogger<Fast2SmsService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public Task<bool> SendOtpAsync(string phoneNumber, string otpCode, string message = "")
    {
        var body = string.IsNullOrEmpty(message) ? $"Your verification code is {otpCode}" : message;
        return SendAsync(phoneNumber, body);
    }

    public async Task<bool> SendAsync(string phoneNumber, string message)
    {
        try
        {
            var apiKey = _configuration["Fast2SMS:ApiKey"];
            var route = _configuration["Fast2SMS:Route"] ?? "q";

            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("Fast2SMS API key not found. SMS will be logged instead of sent.");
                _logger.LogInformation("SMS for {Phone}: {Message} (DEVELOPMENT MODE - NO REAL SMS SENT)", phoneNumber, message);
                return true;
            }

            var cleanPhone = phoneNumber.Replace("+91", "").Replace("+", "").Replace("-", "").Replace(" ", "");
            var requestData = new
            {
                route = route,
                message = message,
                language = "english",
                flash = 0,
                numbers = cleanPhone
            };

            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("authorization", apiKey);

            var response = await _httpClient.PostAsync("https://www.fast2sms.com/dev/bulkV2", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("SMS sent successfully to {Phone} via Fast2SMS", phoneNumber);
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to send SMS to {Phone}. Fast2SMS error: {Error}", phoneNumber, errorContent);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending SMS to {Phone} via Fast2SMS", phoneNumber);
            return false;
        }
    }
}

public class SmsService : ISmsService
{
    private readonly TwoFactorSmsService _twoFactorService;
    private readonly TwilioSmsService _twilioService;
    private readonly Fast2SmsService _fast2SmsService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmsService> _logger;

    public SmsService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<SmsService> logger,
        ILogger<TwoFactorSmsService> twoFactorLogger,
        ILogger<TwilioSmsService> twilioLogger,
        ILogger<Fast2SmsService> fast2SmsLogger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _twoFactorService = new TwoFactorSmsService(httpClient, configuration, twoFactorLogger);
        _twilioService = new TwilioSmsService(httpClient, configuration, twilioLogger);
        _fast2SmsService = new Fast2SmsService(httpClient, configuration, fast2SmsLogger);
    }

    private readonly HttpClient _httpClient;

    public async Task<bool> SendAsync(string phoneNumber, string message)
    {
        var provider = GetProvider();
        _logger.LogInformation("Dispatching SMS via provider: {Provider}", provider);

        switch (provider.ToLowerInvariant())
        {
            case "twilio":
                return await _twilioService.SendAsync(phoneNumber, message);
            case "fast2sms":
                return await _fast2SmsService.SendAsync(phoneNumber, message);
            case "twofactor":
            case "2factor":
            default:
                return await _twoFactorService.SendAsync(phoneNumber, message);
        }
    }

    public async Task<bool> SendOtpAsync(string phoneNumber, string otpCode, string message = "")
    {
        var provider = GetProvider();
        _logger.LogInformation("Dispatching OTP via provider: {Provider}", provider);

        switch (provider.ToLowerInvariant())
        {
            case "twilio":
                return await _twilioService.SendOtpAsync(phoneNumber, otpCode, message);
            case "fast2sms":
                return await _fast2SmsService.SendOtpAsync(phoneNumber, otpCode, message);
            case "twofactor":
            case "2factor":
            default:
                return await _twoFactorService.SendOtpAsync(phoneNumber, otpCode, message);
        }
    }

    private string GetProvider()
    {
        return _configuration["Sms:Provider"]
            ?? Environment.GetEnvironmentVariable("SMS_PROVIDER")
            ?? Environment.GetEnvironmentVariable("Sms__Provider")
            ?? "TwoFactor";
    }
}
