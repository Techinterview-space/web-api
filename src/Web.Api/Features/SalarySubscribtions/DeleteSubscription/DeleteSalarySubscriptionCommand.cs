using System;

namespace Web.Api.Features.SalarySubscribtions.DeleteSubscription;

public record DeleteSalarySubscriptionCommand
{
    public DeleteSalarySubscriptionCommand(
        Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }
}