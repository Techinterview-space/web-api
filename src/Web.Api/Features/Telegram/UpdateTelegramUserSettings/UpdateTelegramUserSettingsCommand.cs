using System;
using Web.Api.Features.Telegram.GetTelegramUserSettings;

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