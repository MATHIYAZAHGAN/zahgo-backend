using System.Diagnostics;
using System.Net;
using System.Text.Json;
using ZAH.Application.Exceptions;
using ZAH.Shared.Constants;
using ZAH.Shared.Responses;

namespace ZAH.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
        
        _logger.LogError(exception, "An error occurred. TraceId: {TraceId}", traceId);

        var response = context.Response;
        response.ContentType = "application/json";

        ApiResponse apiResponse;

        switch (exception)
        {
            case ValidationException validationEx:
                response.StatusCode = validationEx.StatusCode;
                apiResponse = new ApiResponse
                {
                    Success = false,
                    Message = validationEx.Message,
                    Errors = validationEx.ValidationErrors,
                    TraceId = traceId
                };
                break;

            case NotFoundException notFoundEx:
                response.StatusCode = notFoundEx.StatusCode;
                apiResponse = new ApiResponse
                {
                    Success = false,
                    Message = notFoundEx.Message,
                    Errors = new List<string> { notFoundEx.ErrorCode },
                    TraceId = traceId
                };
                break;

            case UnauthorizedException unauthorizedEx:
                response.StatusCode = unauthorizedEx.StatusCode;
                apiResponse = new ApiResponse
                {
                    Success = false,
                    Message = unauthorizedEx.Message,
                    Errors = new List<string> { unauthorizedEx.ErrorCode },
                    TraceId = traceId
                };
                break;

            case ForbiddenException forbiddenEx:
                response.StatusCode = forbiddenEx.StatusCode;
                apiResponse = new ApiResponse
                {
                    Success = false,
                    Message = forbiddenEx.Message,
                    Errors = new List<string> { forbiddenEx.ErrorCode },
                    TraceId = traceId
                };
                break;

            case BusinessRuleException businessEx:
                response.StatusCode = businessEx.StatusCode;
                apiResponse = new ApiResponse
                {
                    Success = false,
                    Message = businessEx.Message,
                    Errors = new List<string> { businessEx.ErrorCode },
                    TraceId = traceId
                };
                break;

            case ConflictException conflictEx:
                response.StatusCode = conflictEx.StatusCode;
                apiResponse = new ApiResponse
                {
                    Success = false,
                    Message = conflictEx.Message,
                    Errors = new List<string> { conflictEx.ErrorCode },
                    TraceId = traceId
                };
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                apiResponse = new ApiResponse
                {
                    Success = false,
                    Message = "An unexpected error occurred. Please try again later.",
                    Errors = new List<string> { ErrorCodes.INTERNAL_SERVER_ERROR },
                    TraceId = traceId
                };
                break;
        }

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var jsonResponse = JsonSerializer.Serialize(apiResponse, options);
        await response.WriteAsync(jsonResponse);
    }
}
