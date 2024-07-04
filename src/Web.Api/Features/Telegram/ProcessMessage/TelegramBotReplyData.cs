using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Web.Api.Features.Telegram.ProcessMessage;

public record TelegramBotReplyData
{
    public TelegramBotReplyData(
        string replyText,
        IReplyMarkup inlineKeyboardMarkup = null,
        ParseMode parseMode = ParseMode.Html)
    {
        ReplyText = replyText;
        InlineKeyboardMarkup = inlineKeyboardMarkup;
        ParseMode = parseMode;
    }

    public string ReplyText { get; }

    public ParseMode ParseMode { get; }

    public IReplyMarkup InlineKeyboardMarkup { get; }
}