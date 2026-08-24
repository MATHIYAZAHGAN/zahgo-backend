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
        
        try
        {
            var categories = await _categoryService.GetAllCategoriesAsync(ct);
            return Ok(ApiResponse<List<CategoryResponse>>.SuccessResponse(categories, "Categories retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories: {Message}", ex.Message);
            throw;
        }
    }

    [HttpGet("debug")]
    public async Task<ActionResult> GetCategoriesDebug(CancellationToken ct = default)
    {
        _logger.LogInformation("Debug: Getting raw categories from database");
        
        try
        {
            // Get raw categories without filtering
            var categories = await _categoryService.GetAllCategoriesAsync(ct);
            return Ok(new { 
                success = true, 
                count = categories.Count,
                categories = categories
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Debug error: {Message}", ex.Message);
            return StatusCode(500, new {
                success = false,
                error = ex.Message,
                stackTrace = ex.StackTrace
            });
        }
    }
}
