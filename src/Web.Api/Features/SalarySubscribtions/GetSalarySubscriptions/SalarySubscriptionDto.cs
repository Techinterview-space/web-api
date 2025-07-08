using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Domain.Entities.StatData;
using Domain.Entities.StatData.Salary;

namespace Web.Api.Features.SalarySubscribtions.GetSalarySubscriptions;

public record SalarySubscriptionDto
{
    public SalarySubscriptionDto()
    {
    }

    public SalarySubscriptionDto(
        StatDataChangeSubscription entity)
    {
        Id = entity.Id;
        Name = entity.Name;
        TelegramChatId = entity.TelegramChatId;
        ProfessionIds = entity.ProfessionIds;
        PreventNotificationIfNoDifference = entity.PreventNotificationIfNoDifference;
        LastMessageSent = entity.StatDataChangeSubscriptionTgMessages is { Count: > 0 }
            ? entity.StatDataChangeSubscriptionTgMessages.Last().CreatedAt
            : null;

        Regularity = entity.Regularity;
        UseAiAnalysis = entity.UseAiAnalysis;
        DeletedAt = entity.DeletedAt;
        CreatedAt = entity.CreatedAt;
        UpdatedAt = entity.UpdatedAt;
    }

    public Guid Id { get; init; }

    public string Name { get; init; }

    public long TelegramChatId { get; init; }

    public List<long> ProfessionIds { get; init; }

    public bool PreventNotificationIfNoDifference { get; init; }

    public bool UseAiAnalysis { get; init; }

    public SubscriptionRegularityType Regularity { get; init; }

    public DateTimeOffset? LastMessageSent { get; init; }

    public DateTimeOffset? DeletedAt { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }

    public static readonly Expression<Func<StatDataChangeSubscription, SalarySubscriptionDto>> Transform = x => new SalarySubscriptionDto
    {
        Id = x.Id,
        Name = x.Name,
        TelegramChatId = x.TelegramChatId,
        ProfessionIds = x.ProfessionIds,
        PreventNotificationIfNoDifference = x.PreventNotificationIfNoDifference,
        LastMessageSent = x.StatDataChangeSubscriptionTgMessages != null && x.StatDataChangeSubscriptionTgMessages.Count > 0
            ? x.StatDataChangeSubscriptionTgMessages
                .OrderBy(z => z.CreatedAt)
                .Last().CreatedAt
            : null,
        UseAiAnalysis = x.UseAiAnalysis,
        Regularity = x.Regularity,
        DeletedAt = x.DeletedAt,
        CreatedAt = x.CreatedAt,
        UpdatedAt = x.UpdatedAt,
    };
}