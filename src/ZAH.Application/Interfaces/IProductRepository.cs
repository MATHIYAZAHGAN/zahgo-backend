using ZAH.Domain.Entities;
using ZAH.Shared.Responses;

namespace ZAH.Application.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<Product?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<(List<Product> items, long totalCount)> GetAllAsync(int page, int pageSize, string? category = null, CancellationToken ct = default);
    Task<List<Product>> GetFeaturedAsync(int count, CancellationToken ct = default);
    Task<Product> CreateAsync(Product product, CancellationToken ct = default);
    Task UpdateAsync(Product product, CancellationToken ct = default);
}

public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync(CancellationToken ct = default);
    Task<Category?> GetBySlugAsync(string slug, CancellationToken ct = default);
}

public interface ICartRepository
{
    Task<Cart?> GetByUserIdAsync(string userId, CancellationToken ct = default);
    Task<Cart> CreateAsync(Cart cart, CancellationToken ct = default);
    Task UpdateAsync(Cart cart, CancellationToken ct = default);
    Task DeleteAsync(string id, CancellationToken ct = default);
}

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<List<Order>> GetByUserIdAsync(string userId, CancellationToken ct = default);
    Task<Order> CreateAsync(Order order, CancellationToken ct = default);
    Task UpdateAsync(Order order, CancellationToken ct = default);
}
