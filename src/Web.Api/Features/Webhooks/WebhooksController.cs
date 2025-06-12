using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Web.Api.Features.Webhooks;

[ApiController]
[Route("api/webhooks")]
public class WebhooksController : ControllerBase
{
    private readonly ILogger<WebhooksController> _logger;

    public WebhooksController(
        ILogger<WebhooksController> logger)
    {
        _logger = logger;
    }

    [HttpPost("sendgrid")]
    public IActionResult SendgridWebhook()
    {
        var bodyStream = new StreamReader(Request.Body);
        bodyStream.BaseStream.Seek(0, SeekOrigin.Begin);
        var bodyText = bodyStream.ReadToEnd();

        _logger.LogInformation(
            "Sendgrid webhook. Body: {Body}",
            bodyText);

        return Ok();
    }
}