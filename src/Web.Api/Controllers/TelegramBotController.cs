using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TechInterviewer.Controllers;

[ApiController]
[Route("telegram")]
public class TelegramBotController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public TelegramBotController(
        IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> AcceptWebhook(
        Update updateRequest)
    {
        if (updateRequest.Message is null)
        {
            return Ok();
        }

        var token = _configuration["Telegram:BotToken"];
        if (string.IsNullOrEmpty(token))
        {
            return BadRequest("Token is not set");
        }

        var client = new TelegramBotClient(token);
        var chatId = updateRequest.Message.Chat.Id;

        await client.SendTextMessageAsync(chatId, "Hello " + updateRequest.Message.From?.Username ?? "stranger");
        return Ok("Message sent!");
    }
}