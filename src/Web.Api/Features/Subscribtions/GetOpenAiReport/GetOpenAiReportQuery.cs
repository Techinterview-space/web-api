using System;
using Infrastructure.Services.OpenAi.Models;
using MediatR;

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