namespace ZAH.Shared.Constants;

public static class CacheKeys
{
    public const string Categories = "categories:all";
    public const string Brands = "brands:all";
    public const string PopularProducts = "products:popular";
    public const string NewArrivals = "products:new";
    public const string BestSellers = "products:bestsellers";
    
    public static string Product(string id) => $"product:{id}";
    public static string ProductBySlug(string slug) => $"product:slug:{slug}";
    public static string Category(string id) => $"category:{id}";
    public static string UserCart(string userId) => $"cart:user:{userId}";
    public static string UserWishlist(string userId) => $"wishlist:user:{userId}";
}
