using System;

namespace Web.Api.Features.Telegram.BotConfigurations;

public record UpdateTelegramBotConfigurationCommand
    : UpdateTelegramBotConfigurationRequest
{
    public UpdateTelegramBotConfigurationCommand(
        Guid id,
        UpdateTelegramBotConfigurationRequest body)
    {
        Id = id;
        DisplayName = body.DisplayName;
        BotUsername = body.BotUsername;
        IsEnabled = body.IsEnabled;
        Token = body.Token;
    }

    public Guid Id { get; }
}
