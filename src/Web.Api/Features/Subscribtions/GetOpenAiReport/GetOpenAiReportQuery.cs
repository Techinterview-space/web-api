using System;
using Infrastructure.Services.OpenAi.Models;

namespace Web.Api.Features.Subscribtions.GetOpenAiReport;

public record GetOpenAiReportQuery
{
    public GetOpenAiReportQuery(
        Guid subscriptionId)
    {
        SubscriptionId = subscriptionId;
    }

    public Guid SubscriptionId { get; }
}