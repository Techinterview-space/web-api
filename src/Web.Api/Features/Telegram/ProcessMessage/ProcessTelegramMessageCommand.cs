using Telegram.Bot;
using Telegram.Bot.Types;

namespace Web.Api.Features.Telegram.ProcessMessage;

public record ProcessTelegramMessageCommand : MediatR.IRequest<string>
{
    public ProcessTelegramMessageCommand(
        ITelegramBotClient client,
        Update updateRequest)
    {
        BotClient = client;
        UpdateRequest = updateRequest;
    }

    public ITelegramBotClient BotClient { get; }

    public Update UpdateRequest { get; }
}