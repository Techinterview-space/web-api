using System.Threading;
using System.Threading.Tasks;
using Domain.Telegram;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace TechInterviewer.Controllers;

[ApiController]
[Route("telegram")]
public class TelegramBotController : ControllerBase
{
    private readonly TelegramBotService _telegram;

    public TelegramBotController(
        TelegramBotService telegram)
    {
        _telegram = telegram;
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> AcceptWebhook(
        [FromBody] Update updateRequest,
        CancellationToken cancellationToken)
    {
        if (updateRequest.Message is null)
        {
            return Ok();
        }

        await _telegram.ProcessMessageAsync(updateRequest, cancellationToken);
        return Ok("Message sent!");
    }
}