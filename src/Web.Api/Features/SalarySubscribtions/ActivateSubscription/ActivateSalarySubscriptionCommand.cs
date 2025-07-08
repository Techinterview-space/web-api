using System;

namespace Web.Api.Features.SalarySubscribtions.ActivateSubscription;

public record ActivateSalarySubscriptionCommand
{
    public ActivateSalarySubscriptionCommand(
        Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }
}