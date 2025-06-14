using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
        var timestamp = Request.Headers["X-Twilio-Email-Event-Webhook-Timestamp"].ToString();

        // Read body first for signature verification
        var (allItems, rawBody) = await TryParseBodyAsync(cancellationToken);

        // Verify signature using the raw body
        if (!VerifySignature(signature, rawBody, timestamp))
        {
            _logger.LogWarning(
                "SendGrid webhook signature verification failed. Signature: {Signature}",
                signature);

            return Ok();
        }

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

    private async Task<(List<SendgridEventItem> Items, string RawBody)> TryParseBodyAsync(
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

                return (new List<SendgridEventItem>(0), rawBody);
            }

            return (items, rawBody);
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "Failed to parse Sendgrid webhook body. Raw body: {RawBody}",
                rawBody);

            return (new List<SendgridEventItem>(0), rawBody);
        }
    }

    private bool VerifySignature(
        string signatureFromHeaders,
        string requestBody,
        string timestamp)
    {
        var signatureFromConfigs = _configuration["SendGridWebhookSignature"];
        if (string.IsNullOrEmpty(signatureFromConfigs))
        {
            _logger.LogWarning("SendGridWebhookSignature is not configured");
            return false;
        }

        if (string.IsNullOrEmpty(signatureFromHeaders) ||
            string.IsNullOrEmpty(requestBody))
        {
            return false;
        }

        try
        {
            // Compute HMAC-SHA256 of the request body using the secret key
            var keyBytes = Encoding.UTF8.GetBytes(signatureFromConfigs);
            var bodyBytes = Encoding.UTF8.GetBytes(timestamp + requestBody);

            using var hmac = new HMACSHA256(keyBytes);
            var computedHashBytes = hmac.ComputeHash(bodyBytes);
            var computedSignature = Convert.ToBase64String(computedHashBytes);

            // Compare the computed signature with the one from headers
            return signatureFromHeaders.Equals(computedSignature, StringComparison.Ordinal);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error computing HMAC signature for webhook verification. Raw body: {RawBody}",
                requestBody);

            return false;
        }
    }
}