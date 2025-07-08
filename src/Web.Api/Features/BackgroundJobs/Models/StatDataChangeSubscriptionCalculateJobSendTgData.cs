using System;
using Domain.Entities.StatData;
using Domain.Entities.StatData.Salary;

namespace Web.Api.Features.BackgroundJobs.Models;

public record StatDataChangeSubscriptionCalculateJobSendTgData
{
    public Exception RaisedException { get; init; }

    public (StatDataChangeSubscription Subscription, long ChatId) SubscriptionToBeUpdated { get; init; }

    public bool HasError => RaisedException != null;

    public bool HasSubscription => SubscriptionToBeUpdated != default;
}