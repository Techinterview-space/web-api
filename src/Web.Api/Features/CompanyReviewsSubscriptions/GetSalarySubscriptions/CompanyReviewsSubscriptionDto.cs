using System;
using System.Linq;
using System.Linq.Expressions;
using Domain.Entities.StatData;
using Domain.Entities.StatData.CompanyReviews;
using Domain.Entities.StatData.Salary;

namespace Web.Api.Features.CompanyReviewsSubscriptions.GetSalarySubscriptions;

public record CompanyReviewsSubscriptionDto
{
    public CompanyReviewsSubscriptionDto()
    {
    }

    public CompanyReviewsSubscriptionDto(
        LastWeekCompanyReviewsSubscription entity)
    {
        Id = entity.Id;
        Name = entity.Name;
        TelegramChatId = entity.TelegramChatId;
        LastMessageSent = entity.TelegramMessages is { Count: > 0 }
            ? entity.TelegramMessages.Last().CreatedAt
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

    public bool UseAiAnalysis { get; init; }

    public SubscriptionRegularityType Regularity { get; init; }

    public DateTimeOffset? LastMessageSent { get; init; }

    public DateTimeOffset? DeletedAt { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }

    public static readonly Expression<Func<LastWeekCompanyReviewsSubscription, CompanyReviewsSubscriptionDto>> Transform = x => new CompanyReviewsSubscriptionDto
    {
        Id = x.Id,
        Name = x.Name,
        TelegramChatId = x.TelegramChatId,
        LastMessageSent = x.TelegramMessages != null && x.TelegramMessages.Count > 0
            ? x.TelegramMessages
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