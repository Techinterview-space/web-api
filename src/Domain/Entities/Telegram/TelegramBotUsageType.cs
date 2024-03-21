namespace Domain.Entities.Telegram;

public enum TelegramBotUsageType
{
    Undefined = 0,

    DirectMessage = 1,

    ChannelMention = 2,

    InlineQuery = 3,
}