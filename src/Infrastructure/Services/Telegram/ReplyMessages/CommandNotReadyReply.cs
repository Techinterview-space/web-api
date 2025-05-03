using Telegram.Bot.Types.Enums;

namespace Infrastructure.Services.Telegram.ReplyMessages;

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