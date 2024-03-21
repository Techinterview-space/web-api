namespace Domain.Entities.Telegram;

public enum TelegramBotUsageType
{
    Undefined = 0,

    DirectMessage = 1,

    GroupMention = 2,

    SupergroupMention = 3,

    InlineQuery = 4,
}