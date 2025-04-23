using System;
using MediatR;

namespace Web.Api.Features.Subscribtions.ActivateSubscription;

public record ActivateStatDataChangeSubscriptionCommand : IRequest<Unit>
{
    public ActivateStatDataChangeSubscriptionCommand(
        Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }
}