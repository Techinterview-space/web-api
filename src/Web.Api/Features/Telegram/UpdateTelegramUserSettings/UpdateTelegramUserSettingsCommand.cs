using System;

namespace Web.Api.Features.Telegram.UpdateTelegramUserSettings;

public record UpdateTelegramUserSettingsCommand
    : UpdateTelegramUserSettingsBody
{
    public UpdateTelegramUserSettingsCommand(
        Guid id,
        UpdateTelegramUserSettingsBody body)
    {
        Id = id;
        SendBotRegularStatsUpdates = body.SendBotRegularStatsUpdates;
    }

    public Guid Id { get; }
}