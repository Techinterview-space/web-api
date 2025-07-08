using System;

namespace Web.Api.Features.SalarySubscribtions.SendUpdatesToSubscriptionChat;

public record SendUpdatesToSalarySubscriptionChatCommand
{
    public SendUpdatesToSalarySubscriptionChatCommand(
        Guid subscriptionId)
    {
        SubscriptionId = subscriptionId;
    }

    public Guid SubscriptionId { get; }
}