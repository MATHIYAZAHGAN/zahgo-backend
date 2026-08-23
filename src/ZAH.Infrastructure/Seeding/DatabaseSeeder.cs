using MongoDB.Driver;
using ZAH.Domain.Entities;
using ZAH.Domain.Enums;
using ZAH.Infrastructure.MongoDB;

namespace ZAH.Infrastructure.Seeding;

public class DatabaseSeeder
{
    private readonly MongoDbContext _context;

    public DatabaseSeeder(MongoDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        // Check if already seeded
        var productCount = await _context.Products.CountDocumentsAsync(_ => true);
        if (productCount > 0)
        {
            Console.WriteLine("Database already seeded. Skipping seeding.");
            return;
        }

        Console.WriteLine("Starting database seeding...");

        // Seed Categories
        await SeedCategoriesAsync();
        
        // Seed Products
        await SeedProductsAsync();

        // Seed Coupons
        await SeedCouponsAsync();

        // Create indexes
        await CreateIndexesAsync();

        Console.WriteLine("Database seeding completed successfully!");
    }

    private async Task SeedCategoriesAsync()
    {
        var categories = new List<Category>
        {
            new() { Name = "Electronics", Slug = "electronics", Icon = "zap", ItemCount = 128, Image = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?auto=format&fit=crop&w=600&q=80", DisplayOrder = 1, IsActive = true },
            new() { Name = "Men Fashion", Slug = "men-fashion", Icon = "user", ItemCount = 215, Image = "https://images.unsplash.com/photo-1490578474895-699cd4e2cf59?auto=format&fit=crop&w=600&q=80", DisplayOrder = 2, IsActive = true },
            new() { Name = "Women Fashion", Slug = "women-fashion", Icon = "heart", ItemCount = 340, Image = "https://images.unsplash.com/photo-1483985988355-763728e1935b?auto=format&fit=crop&w=600&q=80", DisplayOrder = 3, IsActive = true },
            new() { Name = "Home & Living", Slug = "home-living", Icon = "home", ItemCount = 95, Image = "https://images.unsplash.com/photo-1513694203232-719a280e022f?auto=format&fit=crop&w=600&q=80", DisplayOrder = 4, IsActive = true },
            new() { Name = "Accessories", Slug = "accessories", Icon = "watch", ItemCount = 180, Image = "https://images.unsplash.com/photo-1523275335684-37898b6baf30?auto=format&fit=crop&w=600&q=80", DisplayOrder = 5, IsActive = true },
            new() { Name = "Footwear", Slug = "footwear", Icon = "shopping-bag", ItemCount = 142, Image = "https://images.unsplash.com/photo-1542291026-7eec264c27ff?auto=format&fit=crop&w=600&q=80", DisplayOrder = 6, IsActive = true },
            new() { Name = "Fragrance & Beauty", Slug = "beauty-fragrance", Icon = "sparkles", ItemCount = 86, Image = "https://images.unsplash.com/photo-1541643600914-78b084683601?auto=format&fit=crop&w=600&q=80", DisplayOrder = 7, IsActive = true },
            new() { Name = "Sports & Fitness", Slug = "sports-fitness", Icon = "activity", ItemCount = 110, Image = "https://images.unsplash.com/photo-1517838277536-f5f99be501cd?auto=format&fit=crop&w=600&q=80", DisplayOrder = 8, IsActive = true }
        };

        await _context.Categories.InsertManyAsync(categories);
        Console.WriteLine($"Seeded {categories.Count} categories");
    }

    private async Task SeedProductsAsync()
    {
        var products = new List<Product>
        {
            // Electronics
            new()
            {
                Name = "ZAH SoundPro Wireless ANC Headphones",
                Slug = "zah-soundpro-wireless-anc-headphones",
                Brand = "ZAH Audio",
                Category = "Electronics",
                CategoryId = "cat-1",
                Price = 4999,
                OriginalPrice = 7999,
                DiscountPercentage = 38,
                Rating = 4.8,
                ReviewCount = 342,
                Images = new List<string>
                {
                    "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?auto=format&fit=crop&w=800&q=80",
                    "https://images.unsplash.com/photo-1484704849700-f032a568e944?auto=format&fit=crop&w=800&q=80",
                    "https://images.unsplash.com/photo-1546435770-a3e426bf472b?auto=format&fit=crop&w=800&q=80"
                },
                Description = "Immerse yourself in crystal-clear studio audio with active noise cancellation, 40-hour battery life, and ultra-soft memory foam earcups.",
                ShortDescription = "Active Noise Cancelling Headphones with 40-Hour Playtime.",
                InStock = true,
                StockCount = 14,
                Status = ProductStatus.Active,
                IsNew = true,
                IsBestSeller = true,
                IsTrending = true,
                IsFlashSale = true,
                AvailableColors = new List<ProductColor>
                {
                    new() { Name = "Midnight Black", Hex = "#0f172a" },
                    new() { Name = "Silver Slate", Hex = "#94a3b8" },
                    new() { Name = "Champagne Gold", Hex = "#d4af37" }
                },
                Specifications = new List<ProductSpecification>
                {
                    new() { Name = "Bluetooth Version", Value = "5.3" },
                    new() { Name = "Battery Life", Value = "40 Hours (ANC On)" },
                    new() { Name = "Driver Size", Value = "40mm Dynamic" },
                    new() { Name = "Charging Port", Value = "USB Type-C Fast Charge" }
                }
            },
            new()
            {
                Name = "ZAH AirBuds Pro True Wireless Earbuds",
                Slug = "zah-airbuds-pro-true-wireless-earbuds",
                Brand = "ZAH Audio",
                Category = "Electronics",
                CategoryId = "cat-1",
                Price = 2999,
                OriginalPrice = 4999,
                DiscountPercentage = 40,
                Rating = 4.7,
                ReviewCount = 285,
                Images = new List<string>
                {
                    "https://images.unsplash.com/photo-1590658268037-6bf12165a8df?auto=format&fit=crop&w=800&q=80",
                    "https://images.unsplash.com/photo-1606220588913-b3aacb4d2f46?auto=format&fit=crop&w=800&q=80"
                },
                Description = "Compact spatial audio earbuds with adaptive transparency mode, IPX5 water resistance, and wireless charging case.",
                ShortDescription = "TWS Spatial Earbuds with Wireless Charging.",
                InStock = true,
                StockCount = 30,
                Status = ProductStatus.Active,
                IsFlashSale = true,
                AvailableColors = new List<ProductColor>
                {
                    new() { Name = "Pearl White", Hex = "#f8fafc" },
                    new() { Name = "Onyx Black", Hex = "#0f172a" }
                }
            },
            // Accessories
            new()
            {
                Name = "ZAH Minimalist Luxe Chronograph Watch",
                Slug = "zah-minimalist-luxe-chronograph-watch",
                Brand = "ZAH Timepieces",
                Category = "Accessories",
                CategoryId = "cat-5",
                Price = 6499,
                OriginalPrice = 9999,
                DiscountPercentage = 35,
                Rating = 4.9,
                ReviewCount = 218,
                Images = new List<string>
                {
                    "https://images.unsplash.com/photo-1523275335684-37898b6baf30?auto=format&fit=crop&w=800&q=80",
                    "https://images.unsplash.com/photo-1524805444758-089113d48a6d?auto=format&fit=crop&w=800&q=80"
                },
                Description = "Precision Japanese quartz movement housed in scratch-resistant sapphire crystal casing with a genuine Italian leather strap.",
                ShortDescription = "Sapphire Crystal Quartz Watch with Italian Leather Strap.",
                InStock = true,
                StockCount = 8,
                Status = ProductStatus.Active,
                IsBestSeller = true,
                IsTrending = true,
                AvailableColors = new List<ProductColor>
                {
                    new() { Name = "Classic Tan", Hex = "#8b5cf6" },
                    new() { Name = "Onyx Black", Hex = "#18181b" }
                }
            },
            // Men Fashion
            new()
            {
                Name = "ZAH Executive Merino Wool Blazer",
                Slug = "zah-executive-merino-wool-blazer",
                Brand = "ZAH Tailored",
                Category = "Men Fashion",
                CategoryId = "cat-2",
                Price = 8999,
                OriginalPrice = 12999,
                DiscountPercentage = 30,
                Rating = 4.7,
                ReviewCount = 154,
                Images = new List<string>
                {
                    "https://images.unsplash.com/photo-1507679799987-c73779587ccf?auto=format&fit=crop&w=800&q=80",
                    "https://images.unsplash.com/photo-1594938298603-c8148c4dae35?auto=format&fit=crop&w=800&q=80"
                },
                Description = "Crafted from 100% fine Merino wool, featuring a tailored modern fit, notch lapel, and pick-stitch details for refined formal dressing.",
                ShortDescription = "100% Fine Merino Wool Tailored Blazer.",
                InStock = true,
                StockCount = 5,
                Status = ProductStatus.Active,
                IsNew = true,
                AvailableSizes = new List<string> { "S", "M", "L", "XL", "XXL" },
                AvailableColors = new List<ProductColor>
                {
                    new() { Name = "Navy Blue", Hex = "#1e3a8a" },
                    new() { Name = "Charcoal Grey", Hex = "#334155" }
                }
            },
            // Women Fashion
            new()
            {
                Name = "ZAH Aura Silk Evening Dress",
                Slug = "zah-aura-silk-evening-dress",
                Brand = "ZAH Couture",
                Category = "Women Fashion",
                CategoryId = "cat-3",
                Price = 7499,
                OriginalPrice = 10999,
                DiscountPercentage = 32,
                Rating = 4.9,
                ReviewCount = 189,
                Images = new List<string>
                {
                    "https://images.unsplash.com/photo-1539109136881-3be0616acf4b?auto=format&fit=crop&w=800&q=80",
                    "https://images.unsplash.com/photo-1496747611176-843222e1e57c?auto=format&fit=crop&w=800&q=80"
                },
                Description = "Elegantly draped mulberry silk midi dress with cowl neckline and adjustable shoulder straps for special occasions.",
                ShortDescription = "100% Mulberry Silk Draped Midi Dress.",
                InStock = true,
                StockCount = 12,
                Status = ProductStatus.Active,
                IsBestSeller = true,
                AvailableSizes = new List<string> { "XS", "S", "M", "L" },
                AvailableColors = new List<ProductColor>
                {
                    new() { Name = "Emerald Green", Hex = "#065f46" },
                    new() { Name = "Ruby Red", Hex = "#991b1b" },
                    new() { Name = "Champagne", Hex = "#d4af37" }
                }
            },
            // Footwear
            new()
            {
                Name = "ZAH Velocity Running Sneakers",
                Slug = "zah-velocity-running-sneakers",
                Brand = "ZAH Athletic",
                Category = "Footwear",
                CategoryId = "cat-6",
                Price = 3999,
                OriginalPrice = 5999,
                DiscountPercentage = 33,
                Rating = 4.6,
                ReviewCount = 420,
                Images = new List<string>
                {
                    "https://images.unsplash.com/photo-1542291026-7eec264c27ff?auto=format&fit=crop&w=800&q=80",
                    "https://images.unsplash.com/photo-1608231387042-66d1773070a5?auto=format&fit=crop&w=800&q=80"
                },
                Description = "Ultra-lightweight mesh knit upper combined with responsive NITRO foam midsole for maximum energy return during marathons and daily runs.",
                ShortDescription = "Responsive Nitrogen-Infused Performance Runners.",
                InStock = true,
                StockCount = 22,
                Status = ProductStatus.Active,
                IsTrending = true,
                IsFlashSale = true,
                AvailableSizes = new List<string> { "UK 7", "UK 8", "UK 9", "UK 10", "UK 11" }
            },
            // Home & Living
            new()
            {
                Name = "ZAH Ceramic Espresso & Brew Station",
                Slug = "zah-ceramic-espresso-brew-station",
                Brand = "ZAH Living",
                Category = "Home & Living",
                CategoryId = "cat-4",
                Price = 11999,
                OriginalPrice = 16999,
                DiscountPercentage = 29,
                Rating = 4.8,
                ReviewCount = 96,
                Images = new List<string>
                {
                    "https://images.unsplash.com/photo-1517668808822-9eaa03afd2af?auto=format&fit=crop&w=800&q=80",
                    "https://images.unsplash.com/photo-1514432324607-a09d9b4aefdd?auto=format&fit=crop&w=800&q=80"
                },
                Description = "19-bar Italian pump espresso machine with integrated milk frother wand and precision thermoblock heating for coffee connoisseurs.",
                ShortDescription = "19-Bar Italian Pump Professional Espresso Machine.",
                InStock = true,
                StockCount = 6,
                Status = ProductStatus.Active,
                IsNew = true
            },
            // Fragrance & Beauty
            new()
            {
                Name = "ZAH Velvet Oud Eau De Parfum 100ml",
                Slug = "zah-velvet-oud-eau-de-parfum-100ml",
                Brand = "ZAH Parfums",
                Category = "Fragrance & Beauty",
                CategoryId = "cat-7",
                Price = 4999,
                OriginalPrice = 6999,
                DiscountPercentage = 28,
                Rating = 4.9,
                ReviewCount = 275,
                Images = new List<string>
                {
                    "https://images.unsplash.com/photo-1541643600914-78b084683601?auto=format&fit=crop&w=800&q=80",
                    "https://images.unsplash.com/photo-1592945403244-b3fbafd7f539?auto=format&fit=crop&w=800&q=80"
                },
                Description = "An intoxicating blend of Cambodian agarwood, Damascus rose, and warm amber resin crafted in Grasse, France.",
                ShortDescription = "Luxurious Niche Oud & Rose Parfum 100ml.",
                InStock = true,
                StockCount = 20,
                Status = ProductStatus.Active,
                IsBestSeller = true,
                IsFlashSale = true
            },
            // Sports & Fitness
            new()
            {
                Name = "ZAH Pro-Form Adjustable Dumbbell Set",
                Slug = "zah-pro-form-adjustable-dumbbell-set",
                Brand = "ZAH Fitness",
                Category = "Sports & Fitness",
                CategoryId = "cat-8",
                Price = 12999,
                OriginalPrice = 18999,
                DiscountPercentage = 31,
                Rating = 4.8,
                ReviewCount = 145,
                Images = new List<string>
                {
                    "https://images.unsplash.com/photo-1517838277536-f5f99be501cd?auto=format&fit=crop&w=800&q=80",
                    "https://images.unsplash.com/photo-1584735935682-2f2b69dff9d2?auto=format&fit=crop&w=800&q=80"
                },
                Description = "Quick-dial weight selection system ranging from 2.5kg to 24kg per dumbbell with heavy-duty cast iron plates.",
                ShortDescription = "2.5kg to 24kg Quick-Dial Adjustable Dumbbells.",
                InStock = true,
                StockCount = 8,
                Status = ProductStatus.Active,
                IsTrending = true
            }
        };

        await _context.Products.InsertManyAsync(products);
        Console.WriteLine($"Seeded {products.Count} products");
    }

    private async Task SeedCouponsAsync()
    {
        var coupons = new List<Coupon>
        {
            new() { Code = "ZAH10", DiscountPercentage = 10, MinOrderAmount = 999, Description = "10% OFF on all orders above ₹999", ExpiryDate = DateTime.UtcNow.AddYears(1), IsActive = true },
            new() { Code = "WELCOME20", DiscountPercentage = 20, MaxDiscount = 2000, MinOrderAmount = 1999, Description = "20% OFF for new ZAH members", ExpiryDate = DateTime.UtcNow.AddYears(1), IsActive = true, IsFirstOrderOnly = true },
            new() { Code = "LUXE15", DiscountPercentage = 15, MaxDiscount = 3000, MinOrderAmount = 4999, Description = "15% OFF on luxury fashion & audio", ExpiryDate = DateTime.UtcNow.AddYears(1), IsActive = true },
            new() { Code = "FLASH25", DiscountPercentage = 25, MaxDiscount = 5000, MinOrderAmount = 9999, Description = "25% OFF on flash sale electronics & watches", ExpiryDate = DateTime.UtcNow.AddYears(1), IsActive = true }
        };

        await _context.Coupons.InsertManyAsync(coupons);
        Console.WriteLine($"Seeded {coupons.Count} coupons");
    }

    private async Task CreateIndexesAsync()
    {
        // Products indexes
        var productIndexes = new[]
        {
            new CreateIndexModel<Product>(Builders<Product>.IndexKeys.Ascending(p => p.Slug), new CreateIndexOptions { Unique = true }),
            new CreateIndexModel<Product>(Builders<Product>.IndexKeys.Ascending(p => p.Category)),
            new CreateIndexModel<Product>(Builders<Product>.IndexKeys.Ascending(p => p.Brand)),
            new CreateIndexModel<Product>(Builders<Product>.IndexKeys.Ascending(p => p.Status)),
            new CreateIndexModel<Product>(Builders<Product>.IndexKeys.Descending(p => p.CreatedAt))
        };
        await _context.Products.Indexes.CreateManyAsync(productIndexes);

        // Categories indexes
        await _context.Categories.Indexes.CreateOneAsync(
            new CreateIndexModel<Category>(Builders<Category>.IndexKeys.Ascending(c => c.Slug), new CreateIndexOptions { Unique = true }));

        // Users index
        await _context.Users.Indexes.CreateOneAsync(
            new CreateIndexModel<User>(Builders<User>.IndexKeys.Ascending(u => u.Email), new CreateIndexOptions { Unique = true }));

        Console.WriteLine("Created database indexes");
    }
}
