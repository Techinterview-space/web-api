using System;

namespace Web.Api.Features.CompanyReviewsSubscriptions.ActivateSubscription;

public record ActivateCompanyReviewsSubscriptionCommand
{
    public ActivateCompanyReviewsSubscriptionCommand(
        Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }
}