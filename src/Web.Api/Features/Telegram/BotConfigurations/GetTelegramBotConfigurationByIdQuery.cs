using System;

namespace Web.Api.Features.Telegram.BotConfigurations;

public record GetTelegramBotConfigurationByIdQuery
{
    public GetTelegramBotConfigurationByIdQuery(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }
}
