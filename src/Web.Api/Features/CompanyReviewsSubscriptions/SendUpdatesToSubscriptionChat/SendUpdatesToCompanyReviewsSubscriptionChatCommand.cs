using System;

namespace Web.Api.Features.CompanyReviewsSubscriptions.SendUpdatesToSubscriptionChat;

public record SendUpdatesToCompanyReviewsSubscriptionChatCommand
{
    public SendUpdatesToCompanyReviewsSubscriptionChatCommand(
        Guid subscriptionId)
    {
        SubscriptionId = subscriptionId;
    }

    public Guid SubscriptionId { get; }
}