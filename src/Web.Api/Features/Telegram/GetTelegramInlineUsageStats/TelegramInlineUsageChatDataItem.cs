namespace Web.Api.Features.Telegram.GetTelegramInlineUsageStats;

public record TelegramInlineUsageChatDataItem
{
    public TelegramInlineUsageChatDataItem(
        long? chatId,
        int count)
    {
        ChatId = chatId;
        Count = count;
    }

    public long? ChatId { get; }

    public int Count { get; }
}