using System;
using MediatR;

namespace Web.Api.Features.Subscribtions.SendUpdatesToSubscriptionChat;

public record SendUpdatesToSubscriptionChatCommand : IRequest<int>
{
    public SendUpdatesToSubscriptionChatCommand(
        Guid subscriptionId)
    {
        SubscriptionId = subscriptionId;
    }

    public Guid SubscriptionId { get; }
}