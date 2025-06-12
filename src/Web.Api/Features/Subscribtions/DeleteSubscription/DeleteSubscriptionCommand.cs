using System;

namespace Web.Api.Features.Subscribtions.DeleteSubscription;

public record DeleteSubscriptionCommand
{
    public DeleteSubscriptionCommand(
        Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }
}