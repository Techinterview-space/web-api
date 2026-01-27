using System;

namespace Web.Api.Features.SalarySubscribtions.GetSalaryAiReport;

public record GetSalaryAiReportQuery
{
    public GetSalaryAiReportQuery(
        Guid subscriptionId)
    {
        SubscriptionId = subscriptionId;
    }

    public Guid SubscriptionId { get; }
}