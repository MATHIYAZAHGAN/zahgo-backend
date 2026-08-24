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
        string dbName = "unknown";
        long productCount = 0;
        string errorMessage = "";

        try
        {
            dbName = _mongoDbContext.Database.DatabaseNamespace.DatabaseName;
            var products = _mongoDbContext.Database.GetCollection<dynamic>("products");
            productCount = await products.CountDocumentsAsync(Builders<dynamic>.Filter.Empty);
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
            version = "1.0.1",
            database = new
            {
                name = dbName,
                status = dbStatus,
                productCount = productCount,
                error = errorMessage
            }
        }, "ZAH API is running"));
    }
}
