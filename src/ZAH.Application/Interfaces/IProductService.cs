using ZAH.Application.DTOs.Products;
using ZAH.Shared.Responses;

namespace ZAH.Application.Interfaces;

public interface IProductService
{
    Task<PagedResponse<ProductListResponse>> GetProductsAsync(int page, int pageSize, string? category, CancellationToken ct);
    Task<ProductResponse?> GetProductBySlugAsync(string slug, CancellationToken ct);
    Task<List<ProductListResponse>> GetFeaturedProductsAsync(int count, CancellationToken ct);
}

public interface ICategoryService
{
    Task<List<DTOs.Categories.CategoryResponse>> GetAllCategoriesAsync(CancellationToken ct);
}
