using System;
using MediatR;

namespace Web.Api.Features.Subscribtions.DeactivateSubscription;

public record DeactivateStatDataChangeSubscriptionCommand : IRequest<Unit>
{
    public DeactivateStatDataChangeSubscriptionCommand(
        Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }
}