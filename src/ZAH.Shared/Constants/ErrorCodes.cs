namespace ZAH.Shared.Constants;

public static class ErrorCodes
{
    // Authentication & Authorization
    public const string UNAUTHORIZED = "UNAUTHORIZED";
    public const string FORBIDDEN = "FORBIDDEN";
    public const string INVALID_CREDENTIALS = "INVALID_CREDENTIALS";
    public const string EMAIL_ALREADY_EXISTS = "EMAIL_ALREADY_EXISTS";
    public const string ACCOUNT_LOCKED = "ACCOUNT_LOCKED";
    public const string INVALID_TOKEN = "INVALID_TOKEN";
    public const string TOKEN_EXPIRED = "TOKEN_EXPIRED";

    // Products
    public const string PRODUCT_NOT_FOUND = "PRODUCT_NOT_FOUND";
    public const string INSUFFICIENT_STOCK = "INSUFFICIENT_STOCK";
    public const string PRODUCT_INACTIVE = "PRODUCT_INACTIVE";
    public const string INVALID_VARIANT = "INVALID_VARIANT";

    // Orders
    public const string ORDER_NOT_FOUND = "ORDER_NOT_FOUND";
    public const string ORDER_CANNOT_BE_CANCELLED = "ORDER_CANNOT_BE_CANCELLED";
    public const string INVALID_ORDER_STATUS = "INVALID_ORDER_STATUS";

    // Cart
    public const string CART_EMPTY = "CART_EMPTY";
    public const string CART_ITEM_NOT_FOUND = "CART_ITEM_NOT_FOUND";
    public const string INVALID_QUANTITY = "INVALID_QUANTITY";

    // Coupons
    public const string INVALID_COUPON = "INVALID_COUPON";
    public const string COUPON_EXPIRED = "COUPON_EXPIRED";
    public const string COUPON_USAGE_LIMIT_REACHED = "COUPON_USAGE_LIMIT_REACHED";
    public const string MIN_ORDER_AMOUNT_NOT_MET = "MIN_ORDER_AMOUNT_NOT_MET";

    // Payment
    public const string PAYMENT_FAILED = "PAYMENT_FAILED";
    public const string PAYMENT_VERIFICATION_FAILED = "PAYMENT_VERIFICATION_FAILED";
    public const string INVALID_PAYMENT_METHOD = "INVALID_PAYMENT_METHOD";

    // General
    public const string VALIDATION_ERROR = "VALIDATION_ERROR";
    public const string NOT_FOUND = "NOT_FOUND";
    public const string INTERNAL_SERVER_ERROR = "INTERNAL_SERVER_ERROR";
    public const string BAD_REQUEST = "BAD_REQUEST";
    public const string DUPLICATE_REQUEST = "DUPLICATE_REQUEST";
}
