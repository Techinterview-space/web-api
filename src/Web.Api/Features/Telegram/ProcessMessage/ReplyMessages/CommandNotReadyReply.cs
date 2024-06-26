using Telegram.Bot.Types.Enums;

namespace Web.Api.Features.Telegram.ProcessMessage.ReplyMessages;

public record CommandNotReadyReply : TelegramBotReplyData
{
    public CommandNotReadyReply()
        : base(
            "Команда еще в разработке. Stay tuned",
            null,
            ParseMode.Html)
    {
    }
}