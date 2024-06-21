using System;
using System.Linq.Expressions;
using Domain.Entities.Telegram;

namespace Web.Api.Features.Telegram.GetTelegramUserSettings;

public record TelegramUserSettingsDto
{
    public TelegramUserSettingsDto()
    {
    }

    public TelegramUserSettingsDto(
        TelegramUserSettings entity)
    {
        Id = entity.Id;
        Username = entity.Username;
        ChatId = entity.ChatId;
        UserId = entity.UserId;
        SendBotRegularStatsUpdates = entity.SendBotRegularStatsUpdates;
        CreatedAt = entity.CreatedAt;
        UpdatedAt = entity.UpdatedAt;
    }

    public Guid Id { get; init; }

    public string Username { get; init; }

    public long ChatId { get; init; }

    public long UserId { get; init; }

    public bool SendBotRegularStatsUpdates { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }

    public static readonly Expression<Func<TelegramUserSettings, TelegramUserSettingsDto>> Transform = x => new TelegramUserSettingsDto
    {
        Id = x.Id,
        UserId = x.UserId,
        Username = x.Username,
        ChatId = x.ChatId,
        SendBotRegularStatsUpdates = x.SendBotRegularStatsUpdates,
        CreatedAt = x.CreatedAt,
        UpdatedAt = x.UpdatedAt,
    };
}