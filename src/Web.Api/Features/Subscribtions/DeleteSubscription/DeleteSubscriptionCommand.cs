using System;
using MediatR;

namespace Web.Api.Features.Subscribtions.DeleteSubscription;

public record DeleteSubscriptionCommand : IRequest<Unit>
{
    public DeleteSubscriptionCommand(
        Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }
}