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

    public Guid Id { get; set; }

    public TelegramBotType BotType { get; set; }

    public string DisplayName { get; set; }

    public string Token { get; set; }

    public bool IsEnabled { get; set; }

    public string BotUsername { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public bool IsAvailableForProcessing()
    {
        return IsEnabled && !string.IsNullOrEmpty(Token);
    }
}
