using System;

namespace Web.Api.Features.CompanyReviewsSubscriptions.DeactivateSubscription;

public record DeactivateCompanyReviewsSubscriptionCommand
{
    public DeactivateCompanyReviewsSubscriptionCommand(
        Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }
}