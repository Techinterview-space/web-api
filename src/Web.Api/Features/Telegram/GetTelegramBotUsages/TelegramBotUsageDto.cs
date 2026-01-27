using System;
using Domain.Entities.Telegram;

namespace Web.Api.Features.Telegram.GetTelegramBotUsages;

public record TelegramBotUsageDto
{
    public Guid Id { get; init; }

    public long? ChatId { get; init; }

    public string Username { get; init; }

    public TelegramBotUsageType UsageType { get; init; }

    public string UsageTypeAsString => UsageType.ToString();

    public bool IsAdmin { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
}