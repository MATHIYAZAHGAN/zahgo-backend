using MongoDB.Driver;
using ZAH.Application.Interfaces;
using ZAH.Domain.Entities;
using ZAH.Domain.Enums;
using ZAH.Infrastructure.MongoDB;

namespace ZAH.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly MongoDbContext _context;

    public ProductRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        return await _context.Products
            .Find(p => p.Id == id && !p.IsDeleted && p.Status == ProductStatus.Active)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Product?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        return await _context.Products
            .Find(p => p.Slug == slug && !p.IsDeleted && p.Status == ProductStatus.Active)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<(List<Product> items, long totalCount)> GetAllAsync(int page, int pageSize, string? category = null, CancellationToken ct = default)
    {
        var filter = Builders<Product>.Filter.And(
            Builders<Product>.Filter.Eq(p => p.IsDeleted, false),
            Builders<Product>.Filter.Eq(p => p.Status, ProductStatus.Active)
        );

        if (!string.IsNullOrEmpty(category))
        {
            filter = Builders<Product>.Filter.And(
                filter,
                Builders<Product>.Filter.Eq(p => p.Category, category)
            );
        }

        var totalCount = await _context.Products.CountDocumentsAsync(filter, cancellationToken: ct);
        
        var items = await _context.Products
            .Find(filter)
            .SortByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<List<Product>> GetFeaturedAsync(int count, CancellationToken ct = default)
    {
        return await _context.Products
            .Find(p => !p.IsDeleted && p.Status == ProductStatus.Active && p.IsBestSeller)
            .Limit(count)
            .ToListAsync(ct);
    }

    public async Task<Product> CreateAsync(Product product, CancellationToken ct = default)
    {
        await _context.Products.InsertOneAsync(product, cancellationToken: ct);
        return product;
    }

    public async Task UpdateAsync(Product product, CancellationToken ct = default)
    {
        product.UpdatedAt = DateTime.UtcNow;
        await _context.Products.ReplaceOneAsync(p => p.Id == product.Id, product, cancellationToken: ct);
    }
}

public class CategoryRepository : ICategoryRepository
{
    private readonly MongoDbContext _context;

    public CategoryRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<List<Category>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Categories
            .Find(c => !c.IsDeleted && c.IsActive)
            .SortBy(c => c.DisplayOrder)
            .ToListAsync(ct);
    }

    public async Task<Category?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        return await _context.Categories
            .Find(c => c.Slug == slug && !c.IsDeleted)
            .FirstOrDefaultAsync(ct);
    }
}

public class CartRepository : ICartRepository
{
    private readonly MongoDbContext _context;

    public CartRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<Cart?> GetByUserIdAsync(string userId, CancellationToken ct = default)
    {
        return await _context.Carts
            .Find(c => c.UserId == userId && !c.IsDeleted)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Cart> CreateAsync(Cart cart, CancellationToken ct = default)
    {
        await _context.Carts.InsertOneAsync(cart, cancellationToken: ct);
        return cart;
    }

    public async Task UpdateAsync(Cart cart, CancellationToken ct = default)
    {
        cart.UpdatedAt = DateTime.UtcNow;
        await _context.Carts.ReplaceOneAsync(c => c.Id == cart.Id, cart, cancellationToken: ct);
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        await _context.Carts.DeleteOneAsync(c => c.Id == id, ct);
    }
}

public class OrderRepository : IOrderRepository
{
    private readonly MongoDbContext _context;

    public OrderRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        return await _context.Orders
            .Find(o => o.Id == id && !o.IsDeleted)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<Order>> GetByUserIdAsync(string userId, CancellationToken ct = default)
    {
        return await _context.Orders
            .Find(o => o.UserId == userId && !o.IsDeleted)
            .SortByDescending(o => o.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<Order> CreateAsync(Order order, CancellationToken ct = default)
    {
        await _context.Orders.InsertOneAsync(order, cancellationToken: ct);
        return order;
    }

    public async Task UpdateAsync(Order order, CancellationToken ct = default)
    {
        order.UpdatedAt = DateTime.UtcNow;
        await _context.Orders.ReplaceOneAsync(o => o.Id == order.Id, order, cancellationToken: ct);
    }
}
