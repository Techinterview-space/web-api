using System;

namespace Web.Api.Features.Subscribtions.ActivateSubscription;

public record ActivateStatDataChangeSubscriptionCommand
{
    public ActivateStatDataChangeSubscriptionCommand(
        Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }
}