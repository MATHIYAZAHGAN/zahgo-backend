using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using ZAH.Infrastructure.MongoDB;
using ZAH.Shared.Responses;

namespace ZAH.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;
    private readonly MongoDbContext _mongoDbContext;

    public HealthController(ILogger<HealthController> logger, MongoDbContext mongoDbContext)
    {
        _logger = logger;
        _mongoDbContext = mongoDbContext;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse>> Get()
    {
        _logger.LogInformation("Health check requested at {Time}", DateTime.UtcNow);

        // Test MongoDB connection
        string dbStatus = "unknown";
        long productCount = 0;
        string errorMessage = "";

        try
        {
            productCount = await _mongoDbContext.Products.CountDocumentsAsync(FilterDefinition<Domain.Entities.Product>.Empty);
            dbStatus = "connected";
        }
        catch (Exception ex)
        {
            dbStatus = "error";
            errorMessage = ex.Message;
            _logger.LogError(ex, "MongoDB connection test failed");
        }
        
        return Ok(ApiResponse.SuccessResponse(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            version = "1.0.2",
            database = new
            {
                status = dbStatus,
                productCount = productCount,
                error = errorMessage
            }
        }, "ZAH API is running"));
    }
}
