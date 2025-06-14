using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Services.Correlation;
using Infrastructure.Services.Mediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StrongGrid;
using StrongGrid.Models;
using StrongGrid.Models.Webhooks;
using Web.Api.Features.Users.UnsubscribeUserFromEmails;

namespace Web.Api.Features.Webhooks;

[ApiController]
[Route("api/webhooks")]
public class WebhooksController : ControllerBase
{
    public const string HeadersSignatureKey = "X-Twilio-Email-Event-Webhook-Signature";
    public const string HeadersTimestampKey = "X-Twilio-Email-Event-Webhook-Timestamp";

    private readonly ILogger<WebhooksController> _logger;
    private readonly IConfiguration _configuration;
    private readonly ICorrelationIdAccessor _correlationIdAccessor;
    private readonly IServiceProvider _serviceProvider;

    public WebhooksController(
        ILogger<WebhooksController> logger,
        IConfiguration configuration,
        ICorrelationIdAccessor correlationIdAccessor,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _configuration = configuration;
        _correlationIdAccessor = correlationIdAccessor;
        _serviceProvider = serviceProvider;
    }

    [HttpPost("sendgrid")]
    public async Task<IActionResult> SendgridWebhook(
        CancellationToken cancellationToken)
    {
        var allItems = await TryParseBodyAsync(cancellationToken);

        // Verify signature using the raw body
        if (allItems.Count == 0)
        {
            _logger.LogWarning(
                "No items found in Sendgrid webhook. CorrelationId: {CorrelationId}",
                _correlationIdAccessor.GetValue());

            return Ok();
        }

        var spamReportEvents = allItems
            .Where(x => x.EventType is EventType.SpamReport)
            .ToList();

        if (spamReportEvents.Count > 0)
        {
            var result = await _serviceProvider.HandleBy<UnsubscribeUserFromEmailsHandler, List<string>, bool>(
                spamReportEvents.Select(x => x.Email).ToList(),
                cancellationToken);

            _logger.LogInformation(
                "Received {Count} spam report events from Sendgrid. Emails: {Emails}. Processed: {Processed}. CorrelationId: {CorrelationId}",
                spamReportEvents.Count,
                string.Join(", ", spamReportEvents.Select(x => x.Email)),
                result,
                _correlationIdAccessor.GetValue());
        }
        else
        {
            _logger.LogInformation(
                "No spam report events found in Sendgrid webhook. Items: {Items}. CorrelationId: {CorrelationId}",
                JsonSerializer.Serialize(allItems),
                _correlationIdAccessor.GetValue());
        }

        return Ok();
    }

    private async Task<List<Event>> TryParseBodyAsync(
        CancellationToken cancellationToken)
    {
        var signatureFromHeaders = Request.Headers[HeadersSignatureKey].ToString();
        var timestamp = Request.Headers[HeadersTimestampKey].ToString();

        var signatureFromConfigs = _configuration["SendGridWebhookSignature"];
        if (string.IsNullOrEmpty(signatureFromConfigs))
        {
            _logger.LogWarning(
                "SendGridWebhookSignature is not configured. CorrelationId: {CorrelationId}",
                _correlationIdAccessor.GetValue());

            return new List<Event>(0);
        }

        try
        {
            var parser = new WebhookParser();
            var events = await parser.ParseSignedEventsWebhookAsync(
                Request.Body,
                signatureFromConfigs,
                signatureFromHeaders,
                timestamp,
                cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            return events.ToList();
        }
        catch (SecurityException e)
        {
            _logger.LogError(
                e,
                "Security error during signed body verification. CorrelationId: {CorrelationId}",
                _correlationIdAccessor.GetValue());

            return new List<Event>(0);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "General error during signed body verification. CorrelationId: {CorrelationId}",
                _correlationIdAccessor.GetValue());

            return new List<Event>(0);
        }
    }
}