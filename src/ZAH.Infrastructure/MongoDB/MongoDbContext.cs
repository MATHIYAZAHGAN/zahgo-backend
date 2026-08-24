using System.Security.Authentication;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ZAH.Domain.Entities;

namespace ZAH.Infrastructure.MongoDB;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        var connectionString = settings.Value.ConnectionString;
        var clientSettings = MongoClientSettings.FromConnectionString(connectionString);
        
        // Configure SSL/TLS for .NET 8 + MongoDB Atlas compatibility
        clientSettings.SslSettings = new SslSettings
        {
            EnabledSslProtocols = SslProtocols.Tls12
        };
        
        // Increase server selection timeout for cloud deployments
        clientSettings.ServerSelectionTimeout = TimeSpan.FromSeconds(30);
        clientSettings.ConnectTimeout = TimeSpan.FromSeconds(30);

        var client = new MongoClient(clientSettings);
        _database = client.GetDatabase(settings.Value.DatabaseName);
    }

    public IMongoCollection<User> Users => _database.GetCollection<User>("users");
    public IMongoCollection<Product> Products => _database.GetCollection<Product>("products");
    public IMongoCollection<Category> Categories => _database.GetCollection<Category>("categories");
    public IMongoCollection<Brand> Brands => _database.GetCollection<Brand>("brands");
    public IMongoCollection<Cart> Carts => _database.GetCollection<Cart>("carts");
    public IMongoCollection<Wishlist> Wishlists => _database.GetCollection<Wishlist>("wishlists");
    public IMongoCollection<Order> Orders => _database.GetCollection<Order>("orders");
    public IMongoCollection<Coupon> Coupons => _database.GetCollection<Coupon>("coupons");
    public IMongoCollection<RefreshToken> RefreshTokens => _database.GetCollection<RefreshToken>("refresh_tokens");
    public IMongoCollection<AuditLog> AuditLogs => _database.GetCollection<AuditLog>("audit_logs");
}
