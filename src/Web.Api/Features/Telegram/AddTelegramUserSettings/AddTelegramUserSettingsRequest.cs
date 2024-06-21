namespace Web.Api.Features.Telegram.AddTelegramUserSettings;

public record AddTelegramUserSettingsRequest
{
    public long UserId { get; init; }

    public string Username { get; init; }

    public long ChatId { get; init; }

    public bool SendBotRegularStatsUpdates { get; init; }
}