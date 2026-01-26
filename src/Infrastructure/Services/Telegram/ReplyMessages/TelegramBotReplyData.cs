using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Infrastructure.Services.Telegram.ReplyMessages;

public record TelegramBotReplyData
{
    public TelegramBotReplyData(
        string replyText,
        InlineKeyboardMarkup inlineKeyboardMarkup = null,
        ParseMode parseMode = ParseMode.Html)
    {
        ReplyText = replyText;
        InlineKeyboardMarkup = inlineKeyboardMarkup;
        ParseMode = parseMode;
    }

    public string ReplyText { get; }

    public ParseMode ParseMode { get; }

    public InlineKeyboardMarkup InlineKeyboardMarkup { get; }
}