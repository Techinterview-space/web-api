using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Domain.Entities.StatData.Salary;

namespace Web.Api.Features.JobPostingMessageSubscriptions;

public record JobPostingMessageSubscriptionDto
{
    public JobPostingMessageSubscriptionDto()
    {
    }

    public JobPostingMessageSubscriptionDto(
        JobPostingMessageSubscription subscription)
    {
        Id = subscription.Id;
        Name = subscription.Name;
        TelegramChatId = subscription.TelegramChatId;
        ProfessionIds = subscription.ProfessionIds ?? new List<long>();
        CreatedAt = subscription.CreatedAt;
        UpdatedAt = subscription.UpdatedAt;
        DeletedAt = subscription.DeletedAt;
    }

    public Guid Id { get; set; }

    public string Name { get; set; }

    public long TelegramChatId { get; set; }

    public List<long> ProfessionIds { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }

    public static readonly Expression<Func<JobPostingMessageSubscription, JobPostingMessageSubscriptionDto>> Transform = x =>
        new JobPostingMessageSubscriptionDto
        {
            Id = x.Id,
            Name = x.Name,
            TelegramChatId = x.TelegramChatId,
            ProfessionIds = x.ProfessionIds,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt,
            DeletedAt = x.DeletedAt,
        };
}