namespace Web.Api.Features.Telegram.BotConfigurations;

public record UpdateTelegramBotConfigurationRequest
{
    public string DisplayName { get; init; }

    public string BotUsername { get; init; }

    public bool IsEnabled { get; init; }

    public string Token { get; init; }
}
