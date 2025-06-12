using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
    public async Task<IActionResult> SendgridWebhook()
    {
        // Enable seeking on the stream
        Request.EnableBuffering();

        // Read the body as a string
        using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
        var rawBody = await reader.ReadToEndAsync();

        // Reset the stream position for further processing if needed
        Request.Body.Position = 0;

        var signature = Request.Headers["X-Twilio-Email-Event-Webhook-Signature"].ToString();

        _logger.LogInformation(
            "Sendgrid webhook. Body: {Body}. Signature: {Signature}",
            rawBody,
            signature);

        return Ok();
    }
}