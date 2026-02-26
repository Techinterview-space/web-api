using System;
using Domain.Entities.Telegram;

namespace Infrastructure.Services.Telegram;

public class TelegramBotConfigurationCacheItem
{
    public TelegramBotConfigurationCacheItem()
    {
    }

    public TelegramBotConfigurationCacheItem(
        TelegramBotConfiguration entity)
    {
        Id = entity.Id;
        BotType = entity.BotType;
        DisplayName = entity.DisplayName;
        Token = entity.Token;
        IsEnabled = entity.IsEnabled;
        BotUsername = entity.BotUsername;
        CreatedAt = entity.CreatedAt;
        UpdatedAt = entity.UpdatedAt;
    }

    public Guid Id { get; init; }

    public TelegramBotType BotType { get; init; }

    public string DisplayName { get; init; }

    public string Token { get; init; }

    public bool IsEnabled { get; init; }

    public string BotUsername { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }

    public bool IsAvailableForProcessing()
    {
        return IsEnabled && !string.IsNullOrEmpty(Token);
    }
}
