using Domain.Entities.Telegram;

namespace Web.Api.Features.Telegram.BotConfigurations;

public record CreateTelegramBotConfigurationRequest
{
    public TelegramBotType BotType { get; init; }

    public string DisplayName { get; init; }

    public string BotUsername { get; init; }

    public bool IsEnabled { get; init; }

    public string Token { get; init; }
}
