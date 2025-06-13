using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Web.Api.Features.Webhooks.Models;

namespace Web.Api.Features.Webhooks;

[ApiController]
[Route("api/webhooks")]
public class WebhooksController : ControllerBase
{
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
        var signature = Request.Headers["X-Twilio-Email-Event-Webhook-Signature"].ToString();

        // Enable buffering to allow multiple reads of the request body
        Request.EnableBuffering();

        // Read the request body for signature verification
        string requestBody;
        using (var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true))
        {
            requestBody = await reader.ReadToEndAsync(cancellationToken);
        }

        // Reset stream position for subsequent reading
        Request.Body.Position = 0;

        // Verify signature
        if (!VerifySignature(signature, requestBody))
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

    private bool VerifySignature(string signature, string requestBody)
    {
        var signatureKey = _configuration["SendGridWebhookSignatureKey"];
        if (string.IsNullOrEmpty(signatureKey))
        {
            _logger.LogWarning("SendGridWebhookSignatureKey is not configured");
            return false;
        }

        if (string.IsNullOrEmpty(signature))
        {
            return false;
        }

        try
        {
            var keyBytes = Encoding.UTF8.GetBytes(signatureKey);
            var bodyBytes = Encoding.UTF8.GetBytes(requestBody);

            using var hmac = new HMACSHA256(keyBytes);
            var computedHash = hmac.ComputeHash(bodyBytes);
            var computedSignature = Convert.ToBase64String(computedHash);

            return string.Equals(signature, computedSignature, StringComparison.Ordinal);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error verifying SendGrid webhook signature");
            return false;
        }
    }
}