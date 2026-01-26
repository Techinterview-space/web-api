using System;

namespace Web.Api.Features.CompanyReviewsSubscriptions.GetOpenAiReportAnalysis;

public record GetCompanyReviewsAiReportAnalysisQuery
{
    public GetCompanyReviewsAiReportAnalysisQuery(
        Guid subscriptionId)
    {
        SubscriptionId = subscriptionId;
    }

    public Guid SubscriptionId { get; }
}