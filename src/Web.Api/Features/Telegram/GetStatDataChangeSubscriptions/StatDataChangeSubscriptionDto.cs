﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Domain.Entities.StatData;

namespace Web.Api.Features.Telegram.GetStatDataChangeSubscriptions;

public record StatDataChangeSubscriptionDto
{
    public StatDataChangeSubscriptionDto()
    {
    }

    public StatDataChangeSubscriptionDto(
        StatDataChangeSubscription entity)
    {
        Id = entity.Id;
        Name = entity.Name;
        TelegramChatId = entity.TelegramChatId;
        ProfessionIds = entity.ProfessionIds;
        DeletedAt = entity.DeletedAt;
        CreatedAt = entity.CreatedAt;
        UpdatedAt = entity.UpdatedAt;
    }

    public Guid Id { get; init; }

    public string Name { get; init; }

    public long TelegramChatId { get; init; }

    public List<long> ProfessionIds { get; init; }

    public DateTimeOffset? DeletedAt { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }

    public static readonly Expression<Func<StatDataChangeSubscription, StatDataChangeSubscriptionDto>> Transform = x => new StatDataChangeSubscriptionDto
    {
        Id = x.Id,
        Name = x.Name,
        TelegramChatId = x.TelegramChatId,
        ProfessionIds = x.ProfessionIds,
        DeletedAt = x.DeletedAt,
        CreatedAt = x.CreatedAt,
        UpdatedAt = x.UpdatedAt,
    };
}