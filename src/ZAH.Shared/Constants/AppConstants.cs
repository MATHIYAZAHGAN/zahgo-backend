namespace ZAH.Shared.Constants;

public static class AppConstants
{
    // Pagination
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    // Cart
    public const int CartExpirationDays = 30;
    public const int MaxCartItems = 50;
    public const int MaxQuantityPerItem = 10;

    // Orders
    public const string OrderNumberPrefix = "ZAH";
    public const decimal FreeShippingThreshold = 1999;
    public const decimal DefaultShippingFee = 99;
    public const decimal DefaultTaxRate = 0.18m; // 18% GST

    // Authentication
    public const int AccessTokenExpirationMinutes = 30;
    public const int RefreshTokenExpirationDays = 30;
    public const int MaxFailedLoginAttempts = 5;
    public const int AccountLockoutMinutes = 30;
    public const int PasswordResetTokenExpirationHours = 24;

    // File Upload
    public const int MaxImageSizeMB = 5;
    public const int MaxImagesPerProduct = 10;
    
    // Rate Limiting
    public const int AuthRateLimitPerMinute = 5;
    public const int ApiRateLimitPerMinute = 60;
}
