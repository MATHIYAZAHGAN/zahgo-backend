using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZAH.Application.DTOs.Payments;
using ZAH.Application.Interfaces;

namespace ZAH.API.Controllers;

[ApiController]
[Route("api/v1/payments")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _payments;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IPaymentService payments, ILogger<PaymentsController> logger)
    {
        _payments = payments;
        _logger = logger;
    }

    [HttpPost("create-order")]
    [Authorize]
    public async Task<IActionResult> CreateOrder([FromBody] CreatePaymentOrderRequest request, CancellationToken ct)
    {
        try
        {
            var userId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();
            var result = await _payments.CreateOrderAsync(userId, request, ct);
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment order: {Message}", ex.Message);
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("status/{orderId}")]
    [Authorize]
    public async Task<IActionResult> Status(string orderId, CancellationToken ct)
    {
        var userId = User.FindFirst("userId")?.Value;
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();
        var result = await _payments.GetStatusAsync(userId, orderId, ct);
        return Ok(new { success = true, data = result });
    }

    [HttpPost("cashfree/webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook(CancellationToken ct)
    {
        Request.EnableBuffering();
        using var reader = new StreamReader(Request.Body);
        var rawBody = await reader.ReadToEndAsync(ct);
        var signature = Request.Headers["x-webhook-signature"].ToString();
        var timestamp = Request.Headers["x-webhook-timestamp"].ToString();
        return await _payments.ProcessWebhookAsync(rawBody, signature, timestamp, ct) ? Ok() : Unauthorized();
    }
}