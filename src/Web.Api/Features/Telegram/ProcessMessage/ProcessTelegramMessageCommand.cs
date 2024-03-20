using Telegram.Bot;
using Telegram.Bot.Types;

namespace TechInterviewer.Features.Telegram.ProcessMessage;

public record ProcessTelegramMessageCommand : MediatR.IRequest<MediatR.Unit>
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