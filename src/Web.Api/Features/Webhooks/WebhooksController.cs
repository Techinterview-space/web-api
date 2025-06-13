using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Web.Api.Features.Webhooks.Models;

namespace Web.Api.Features.Webhooks;

[ApiController]
[Route("api/webhooks")]
public class WebhooksController : ControllerBase
{
    public const string HeadersSignatureKey = "X-Twilio-Email-Event-Webhook-Signature";

    private readonly ILogger<WebhooksController> _logger;
    private readonly IConfiguration _configuration;

    public WebhooksController(
        ILogger<WebhooksController> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    [HttpPost("sendgrid")]
    public async Task<IActionResult> SendgridWebhook(
        CancellationToken cancellationToken)
    {
        var signature = Request.Headers[HeadersSignatureKey].ToString();

        // Verify signature
        if (!VerifySignature(signature))
        {
            _logger.LogWarning(
                "SendGrid webhook signature verification failed. Signature: {Signature}",
                signature);

            return Ok();
        }

        var allItems = await TryParseBodyAsync(cancellationToken);
        var spamReportEvents = allItems
            .Where(x => x.Event == "spamreport")
            .ToList();

        if (spamReportEvents.Count > 0)
        {
            _logger.LogWarning(
                "Received {Count} spam report events from Sendgrid. Signature: {Signature}. Emails: {Emails}",
                spamReportEvents.Count,
                signature,
                string.Join(", ", spamReportEvents.Select(x => x.Email)));
        }
        else
        {
            _logger.LogInformation(
                "No spam report events found in Sendgrid webhook. Signature: {Signature}. Items: {Items}",
                signature,
                JsonSerializer.Serialize(allItems));
        }

        return Ok();
    }

    private async Task<List<SendgridEventItem>> TryParseBodyAsync(
        CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
        var rawBody = await reader.ReadToEndAsync(cancellationToken);

        try
        {
            var items = JsonSerializer.Deserialize<List<SendgridEventItem>>(rawBody);
            if (items == null)
            {
                _logger.LogWarning(
                    "Sendgrid webhook body deserialization returned null. Body {Body}",
                    rawBody);

                return new List<SendgridEventItem>(0);
            }

            return items;
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "Failed to parse Sendgrid webhook body. Raw body: {RawBody}",
                rawBody);

            return new List<SendgridEventItem>(0);
        }
    }

    private bool VerifySignature(
        string signatureFromHeaders)
    {
        var signatureFromConfigs = _configuration["SendGridWebhookSignature"];
        if (string.IsNullOrEmpty(signatureFromConfigs))
        {
            _logger.LogWarning("SendGridWebhookSignature is not configured");
            return false;
        }

        if (string.IsNullOrEmpty(signatureFromHeaders) ||
            string.IsNullOrEmpty(signatureFromConfigs))
        {
            return false;
        }

        return signatureFromHeaders.Equals(signatureFromConfigs, StringComparison.InvariantCultureIgnoreCase);
    }
}