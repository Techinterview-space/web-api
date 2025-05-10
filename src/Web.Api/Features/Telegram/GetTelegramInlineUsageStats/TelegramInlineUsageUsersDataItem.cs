namespace Web.Api.Features.Telegram.GetTelegramInlineUsageStats;

public record TelegramInlineUsageUsersDataItem
{
    public TelegramInlineUsageUsersDataItem(
        string username,
        int count)
    {
        Username = username;
        Count = count;
    }

    public string Username { get; }

    public int Count { get; }
}