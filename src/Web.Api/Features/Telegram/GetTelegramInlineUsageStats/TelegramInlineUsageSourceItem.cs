using System;

namespace Web.Api.Features.Telegram.GetTelegramInlineUsageStats;

public record TelegramInlineUsageSourceItem
{
    public DateTimeOffset CreatedAt { get; init; }

    public string Username { get; init; }

    public long? ChatId { get; init; }

    public string ChatName { get; init; }
}