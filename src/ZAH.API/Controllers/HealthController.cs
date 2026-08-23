using Microsoft.AspNetCore.Mvc;
using ZAH.Shared.Responses;

namespace ZAH.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;

    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public ActionResult<ApiResponse> Get()
    {
        _logger.LogInformation("Health check requested");
        
        return Ok(ApiResponse.SuccessResponse(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        }, "ZAH API is running"));
    }
}
