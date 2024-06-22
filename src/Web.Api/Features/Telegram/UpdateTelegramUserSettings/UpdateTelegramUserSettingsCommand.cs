using System;
using MediatR;
using Web.Api.Features.Telegram.GetTelegramUserSettings;

namespace Web.Api.Features.Telegram.UpdateTelegramUserSettings;

public record UpdateTelegramUserSettingsCommand
    : UpdateTelegramUserSettingsBody, IRequest<TelegramUserSettingsDto>
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