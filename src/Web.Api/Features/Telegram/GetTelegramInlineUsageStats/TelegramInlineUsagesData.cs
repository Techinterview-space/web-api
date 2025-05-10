using System.Collections.Generic;
using System.Linq;

namespace Web.Api.Features.Telegram.GetTelegramInlineUsageStats;

public record TelegramInlineUsagesData
{
    public TelegramInlineUsagesData(
        List<TelegramInlineUsageSourceItem> telegramInlineUsages)
    {
        UsersStats = telegramInlineUsages
            .GroupBy(x => x.Username)
            .Select(x => new TelegramInlineUsageUsersDataItem(
                x.Key,
                x.Count()))
            .ToList();

        ChatsStats = telegramInlineUsages
            .Where(x => x.ChatId.HasValue)
            .GroupBy(x => x.ChatId)
            .Select(x => new TelegramInlineUsageChatDataItem(
                x.Key,
                x.First().ChatName,
                x.Count()))
            .ToList();
    }

    public List<TelegramInlineUsageUsersDataItem> UsersStats { get; }

    public List<TelegramInlineUsageChatDataItem> ChatsStats { get; }
}