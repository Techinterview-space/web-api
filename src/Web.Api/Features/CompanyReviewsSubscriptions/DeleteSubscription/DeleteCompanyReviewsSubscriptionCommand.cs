using System;

namespace Web.Api.Features.CompanyReviewsSubscriptions.DeleteSubscription;

public record DeleteCompanyReviewsSubscriptionCommand
{
    public DeleteCompanyReviewsSubscriptionCommand(
        Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }
}