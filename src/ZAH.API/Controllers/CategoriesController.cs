using Microsoft.AspNetCore.Mvc;
using ZAH.Application.DTOs.Categories;
using ZAH.Application.Interfaces;
using ZAH.Shared.Responses;

namespace ZAH.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CategoryResponse>>>> GetCategories(CancellationToken ct = default)
    {
        _logger.LogInformation("Getting all categories");
        
        var categories = await _categoryService.GetAllCategoriesAsync(ct);
        return Ok(ApiResponse<List<CategoryResponse>>.SuccessResponse(categories, "Categories retrieved successfully"));
    }
}
