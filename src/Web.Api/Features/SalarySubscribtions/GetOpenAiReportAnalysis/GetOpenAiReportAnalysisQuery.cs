using System;

namespace Web.Api.Features.SalarySubscribtions.GetOpenAiReportAnalysis;

public record GetOpenAiReportAnalysisQuery
{
    public GetOpenAiReportAnalysisQuery(
        Guid subscriptionId)
    {
        SubscriptionId = subscriptionId;
    }

    public Guid SubscriptionId { get; }
}