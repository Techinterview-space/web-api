namespace Web.Api.Features.Telegram.UpdateTelegramUserSettings;

public record UpdateTelegramUserSettingsBody
{
    public bool SendBotRegularStatsUpdates { get; init; }
}