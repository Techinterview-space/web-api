using System;
using Web.Api.Features.CompanyReviewsSubscriptions.Shared;
using Web.Api.Features.SalarySubscribtions.Shared;

namespace Web.Api.Features.CompanyReviewsSubscriptions.EditSubscription;

public record EditCompanyReviewsSubscriptionCommand : EditCompanyReviewsSubscriptionBodyRequest
{
    public Guid SubscriptionId { get; }

    public EditCompanyReviewsSubscriptionCommand(
        Guid subscriptionId,
        EditSalarySubscriptionBodyRequest request)
    {
        SubscriptionId = subscriptionId;
        Name = request.Name;
        Regularity = request.Regularity;
        UseAiAnalysis = request.UseAiAnalysis;
    }
}