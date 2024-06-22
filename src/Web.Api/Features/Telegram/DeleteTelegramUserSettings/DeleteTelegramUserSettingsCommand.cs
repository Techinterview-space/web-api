using System;
using MediatR;

namespace Web.Api.Features.Telegram.DeleteTelegramUserSettings;

public record DeleteTelegramUserSettingsCommand
    : IRequest<Unit>
{
    public DeleteTelegramUserSettingsCommand(
        Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }
}