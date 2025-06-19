using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Infrastructure.Services.Telegram;

public static class TelegramBotExtensions
{
    public static Task ReplyToWithHtmlAsync(
        this ITelegramBotClient bot,
        long chatId,
        int messageId,
        string html,
        CancellationToken cancellationToken = default)
    {
        return bot.SendMessage(
            chatId,
            html,
            parseMode: ParseMode.Html,
            replyParameters: new ReplyParameters
            {
                MessageId = messageId,
            },
            cancellationToken: cancellationToken);
    }
}