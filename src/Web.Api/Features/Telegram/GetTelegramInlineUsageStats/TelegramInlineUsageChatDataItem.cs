namespace Web.Api.Features.Telegram.GetTelegramInlineUsageStats;

public class TelegramInlineUsageChatDataItem
{
    public TelegramInlineUsageChatDataItem(
        long? chatId,
        string chatName,
        int count)
    {
        ChatName = chatName;
        ChatId = chatId;
        Count = count;
    }

    public string ChatName { get; }

    public long? ChatId { get; }

    public int Count { get; }
}