namespace Web.Api.Features.Dashboard.Dtos;

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