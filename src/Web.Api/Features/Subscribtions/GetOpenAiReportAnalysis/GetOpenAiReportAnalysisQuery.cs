using System;
using MediatR;

namespace Web.Api.Features.Subscribtions.GetOpenAiReportAnalysis;

public record GetOpenAiReportAnalysisQuery : IRequest<GetOpenAiReportAnalysisResponse>
{
    public GetOpenAiReportAnalysisQuery(
        Guid subscriptionId)
    {
        SubscriptionId = subscriptionId;
    }

    public Guid SubscriptionId { get; }
}