using System;
using Domain.Entities.Telegram;

namespace Web.Api.Features.Telegram.BotConfigurations;

public record TelegramBotConfigurationDto
{
    public TelegramBotConfigurationDto()
    {
    }

    public TelegramBotConfigurationDto(
        TelegramBotConfiguration entity)
    {
        Id = entity.Id;
        BotType = entity.BotType;
        BotTypeAsString = entity.BotType.ToString();
        DisplayName = entity.DisplayName;
        BotUsername = entity.BotUsername;
        IsEnabled = entity.IsEnabled;
        HasToken = !string.IsNullOrEmpty(entity.Token);
        MaskedToken = MaskToken(entity.Token);
        CreatedAt = entity.CreatedAt;
        UpdatedAt = entity.UpdatedAt;
    }

    public Guid Id { get; init; }

    public TelegramBotType BotType { get; init; }

    public string BotTypeAsString { get; init; }

    public string DisplayName { get; init; }

    public string BotUsername { get; init; }

    public bool IsEnabled { get; init; }

    public bool HasToken { get; init; }

    public string MaskedToken { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }

    private static string MaskToken(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return null;
        }

        if (token.Length <= 3)
        {
            return "***";
        }

        return "***" + token[^3..];
    }
}
