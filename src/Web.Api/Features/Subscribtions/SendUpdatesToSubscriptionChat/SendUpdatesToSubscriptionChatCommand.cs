using System;

namespace Web.Api.Features.Subscribtions.SendUpdatesToSubscriptionChat;

public record SendUpdatesToSubscriptionChatCommand
{
    public SendUpdatesToSubscriptionChatCommand(
        Guid subscriptionId)
    {
        SubscriptionId = subscriptionId;
    }

    public Guid SubscriptionId { get; }
}