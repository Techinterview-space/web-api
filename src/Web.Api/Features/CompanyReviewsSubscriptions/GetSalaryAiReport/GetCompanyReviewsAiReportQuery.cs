using System;

namespace Web.Api.Features.CompanyReviewsSubscriptions.GetSalaryAiReport;

public record GetCompanyReviewsAiReportQuery
{
    public GetCompanyReviewsAiReportQuery(
        Guid subscriptionId)
    {
        SubscriptionId = subscriptionId;
    }

    public Guid SubscriptionId { get; }
}