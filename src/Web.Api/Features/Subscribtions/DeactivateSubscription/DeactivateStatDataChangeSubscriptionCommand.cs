using System;

namespace Web.Api.Features.Subscribtions.DeactivateSubscription;

public record DeactivateStatDataChangeSubscriptionCommand
{
    public DeactivateStatDataChangeSubscriptionCommand(
        Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }
}