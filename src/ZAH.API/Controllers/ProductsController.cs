using Microsoft.AspNetCore.Mvc;
using ZAH.Application.DTOs.Products;
using ZAH.Application.Exceptions;
using ZAH.Application.Interfaces;
using ZAH.Shared.Responses;

namespace ZAH.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponse<ProductListResponse>>>> GetProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? category = null,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Getting products - Page: {Page}, PageSize: {PageSize}, Category: {Category}", page, pageSize, category);
        
        var products = await _productService.GetProductsAsync(page, pageSize, category, ct);
        return Ok(ApiResponse<PagedResponse<ProductListResponse>>.SuccessResponse(products, "Products retrieved successfully"));
    }

    [HttpGet("featured")]
    public async Task<ActionResult<ApiResponse<List<ProductListResponse>>>> GetFeaturedProducts(
        [FromQuery] int count = 10,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Getting featured products");
        
        var products = await _productService.GetFeaturedProductsAsync(count, ct);
        return Ok(ApiResponse<List<ProductListResponse>>.SuccessResponse(products, "Featured products retrieved successfully"));
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult<ApiResponse<ProductResponse>>> GetProduct(
        string slug,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Getting product by slug: {Slug}", slug);
        
        var product = await _productService.GetProductBySlugAsync(slug, ct);
        if (product == null)
        {
            throw new NotFoundException($"Product with slug '{slug}' not found");
        }

        return Ok(ApiResponse<ProductResponse>.SuccessResponse(product, "Product retrieved successfully"));
    }
}
