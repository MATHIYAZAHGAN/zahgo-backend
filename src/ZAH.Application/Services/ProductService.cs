using ZAH.Application.DTOs.Products;
using ZAH.Application.Exceptions;
using ZAH.Application.Interfaces;
using ZAH.Shared.Constants;
using ZAH.Shared.Responses;

namespace ZAH.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<PagedResponse<ProductListResponse>> GetProductsAsync(int page, int pageSize, string? category, CancellationToken ct)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > AppConstants.MaxPageSize) 
            pageSize = AppConstants.DefaultPageSize;

        var (items, totalCount) = await _productRepository.GetAllAsync(page, pageSize, category, ct);

        var productResponses = items.Select(p => new ProductListResponse
        {
            Id = p.Id,
            Name = p.Name,
            Slug = p.Slug,
            Brand = p.Brand,
            Category = p.Category,
            Price = p.Price,
            OriginalPrice = p.OriginalPrice,
            DiscountPercentage = p.DiscountPercentage,
            Rating = p.Rating,
            ReviewCount = p.ReviewCount,
            Images = p.Images,
            ShortDescription = p.ShortDescription,
            InStock = p.InStock,
            IsNew = p.IsNew,
            IsBestSeller = p.IsBestSeller,
            IsTrending = p.IsTrending,
            IsFlashSale = p.IsFlashSale,
            AvailableColors = p.AvailableColors,
            AvailableSizes = p.AvailableSizes
        }).ToList();

        return new PagedResponse<ProductListResponse>
        {
            Items = productResponses,
            Page = page,
            PageSize = pageSize,
            TotalCount = (int)totalCount
        };
    }

    public async Task<ProductResponse?> GetProductBySlugAsync(string slug, CancellationToken ct)
    {
        var product = await _productRepository.GetBySlugAsync(slug, ct);
        if (product == null) return null;

        return new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Slug = product.Slug,
            Brand = product.Brand,
            Category = product.Category,
            CategoryId = product.CategoryId,
            Price = product.Price,
            OriginalPrice = product.OriginalPrice,
            DiscountPercentage = product.DiscountPercentage,
            Rating = product.Rating,
            ReviewCount = product.ReviewCount,
            Images = product.Images,
            Description = product.Description,
            ShortDescription = product.ShortDescription,
            InStock = product.InStock,
            StockCount = product.StockCount,
            IsNew = product.IsNew,
            IsBestSeller = product.IsBestSeller,
            IsTrending = product.IsTrending,
            IsFlashSale = product.IsFlashSale,
            Tags = product.Tags,
            AvailableColors = product.AvailableColors,
            AvailableSizes = product.AvailableSizes,
            Variants = product.Variants,
            Specifications = product.Specifications,
            Reviews = product.Reviews
        };
    }

    public async Task<List<ProductListResponse>> GetFeaturedProductsAsync(int count, CancellationToken ct)
    {
        var products = await _productRepository.GetFeaturedAsync(count, ct);
        
        return products.Select(p => new ProductListResponse
        {
            Id = p.Id,
            Name = p.Name,
            Slug = p.Slug,
            Brand = p.Brand,
            Category = p.Category,
            Price = p.Price,
            OriginalPrice = p.OriginalPrice,
            DiscountPercentage = p.DiscountPercentage,
            Rating = p.Rating,
            ReviewCount = p.ReviewCount,
            Images = p.Images,
            ShortDescription = p.ShortDescription,
            InStock = p.InStock,
            IsNew = p.IsNew,
            IsBestSeller = p.IsBestSeller,
            IsTrending = p.IsTrending,
            IsFlashSale = p.IsFlashSale,
            AvailableColors = p.AvailableColors,
            AvailableSizes = p.AvailableSizes
        }).ToList();
    }
}

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<List<DTOs.Categories.CategoryResponse>> GetAllCategoriesAsync(CancellationToken ct)
    {
        var categories = await _categoryRepository.GetAllAsync(ct);
        
        return categories.Select(c => new DTOs.Categories.CategoryResponse
        {
            Id = c.Id,
            Name = c.Name,
            Slug = c.Slug,
            Icon = c.Icon,
            Image = c.Image,
            ItemCount = c.ItemCount
        }).ToList();
    }
}
