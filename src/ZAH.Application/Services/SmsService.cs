using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ZAH.Application.Services;

public interface ISmsService
{
    Task<bool> SendAsync(string phoneNumber, string message);
}

public class SmsService : ISmsService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmsService> _logger;

    public SmsService(HttpClient httpClient, IConfiguration configuration, ILogger<SmsService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SendAsync(string phoneNumber, string message)
    {
        try
        {
            // Get Twilio configuration
            var accountSid = _configuration["Twilio:AccountSid"];
            var authToken = _configuration["Twilio:AuthToken"];
            var fromNumber = _configuration["Twilio:PhoneNumber"];

            if (string.IsNullOrEmpty(accountSid) || string.IsNullOrEmpty(authToken) || string.IsNullOrEmpty(fromNumber))
            {
                _logger.LogWarning("Twilio configuration not found. SMS will be logged instead of sent.");
                _logger.LogInformation("SMS for {Phone}: {Message} (DEVELOPMENT MODE - NO REAL SMS SENT)", phoneNumber, message);
                return true; // Return true for development mode
            }

            // Prepare Twilio API request
            var twilioUrl = $"https://api.twilio.com/2010-04-01/Accounts/{accountSid}/Messages.json";
            
            var parameters = new List<KeyValuePair<string, string>>
            {
                new("To", phoneNumber),
                new("From", fromNumber),
                new("Body", message)
            };

            var encodedContent = new FormUrlEncodedContent(parameters);

            // Add Twilio authentication
            var credentials = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{accountSid}:{authToken}"));
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

            // Send SMS via Twilio API
            var response = await _httpClient.PostAsync(twilioUrl, encodedContent);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
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
            _logger.LogError(ex, "Exception occurred while sending SMS to {Phone}", phoneNumber);
            return false;
        }
    }
}

// Alternative: Fast2SMS Service (Popular in India)
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

    public async Task<bool> SendAsync(string phoneNumber, string message)
    {
        try
        {
            var apiKey = _configuration["Fast2SMS:ApiKey"];
            var route = _configuration["Fast2SMS:Route"] ?? "q"; // Default route

            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("Fast2SMS API key not found. SMS will be logged instead of sent.");
                _logger.LogInformation("SMS for {Phone}: {Message} (DEVELOPMENT MODE - NO REAL SMS SENT)", phoneNumber, message);
                return true;
            }

            // Clean phone number (remove +91 if present)
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
                var responseContent = await response.Content.ReadAsStringAsync();
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