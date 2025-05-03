using System;
using MediatR;
using Web.Api.Integrations.OpenAiAnalysisIntegration;

namespace Web.Api.Features.Subscribtions.GetOpenAiReport;

public record GetOpenAiReportQuery : IRequest<OpenAiBodyReport>
{
    public GetOpenAiReportQuery(
        Guid subscriptionId)
    {
        SubscriptionId = subscriptionId;
    }

    public Guid SubscriptionId { get; }
}