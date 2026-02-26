using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Services.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Web.Api.Features.ChannelStats.Webhook;

[ApiController]
[Route("api/integrations/telegram")]
[AllowAnonymous]
public class ChannelStatsWebhookController : ControllerBase
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ChannelStatsWebhookController> _logger;

    public ChannelStatsWebhookController(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<ChannelStatsWebhookController> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> ReceiveUpdate(
        CancellationToken cancellationToken)
    {
        var expectedSecret = _configuration["Telegram:WebhookSecretToken"];
        if (string.IsNullOrWhiteSpace(expectedSecret))
        {
            _logger.LogError("Telegram webhook secret token is not configured");
            return Unauthorized();
        }

        var headerSecret = Request.Headers["X-Telegram-Bot-Api-Secret-Token"].ToString();
        if (!string.Equals(expectedSecret, headerSecret, StringComparison.Ordinal))
        {
            _logger.LogWarning("Telegram webhook received with invalid secret token");
            return Unauthorized();
        }

        string body;
        using (var reader = new StreamReader(Request.Body))
        {
            body = await reader.ReadToEndAsync(cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            return BadRequest();
        }

        long updateId;
        try
        {
            using var doc = JsonDocument.Parse(body);
            updateId = doc.RootElement.GetProperty("update_id").GetInt64();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse update_id from Telegram webhook payload");
            return BadRequest();
        }

        var request = new ProcessTelegramUpdateRequest(updateId, body);

        await _serviceProvider
            .HandleBy<ProcessTelegramUpdateHandler, ProcessTelegramUpdateRequest, Nothing>(
                request,
                cancellationToken);

        return Ok();
    }
}
