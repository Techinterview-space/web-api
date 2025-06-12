using System;

namespace Web.Api.Features.Telegram.DeleteTelegramUserSettings;

public record DeleteTelegramUserSettingsCommand
   
{
    public DeleteTelegramUserSettingsCommand(
        Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }
}