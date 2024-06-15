using System;
using Domain.Entities.Telegram;

namespace Web.Api.Features.Telegram.GetTelegramBotUsages;

public record TelegramBotUsageDto
{
    public Guid Id { get; init; }

    public long UsageCount { get; init; }

    public string Username { get; init; }

    public string ChannelName { get; init; }

    public string ReceivedMessageText { get; init; }

    public TelegramBotUsageType UsageType { get; init; }

    public string UsageTypeAsString => UsageType.ToString();

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }
}