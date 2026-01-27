using System;

namespace Web.Api.Features.SalarySubscribtions.DeactivateSubscription;

public record DeactivateSalarySubscriptionCommand
{
    public DeactivateSalarySubscriptionCommand(
        Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }
}