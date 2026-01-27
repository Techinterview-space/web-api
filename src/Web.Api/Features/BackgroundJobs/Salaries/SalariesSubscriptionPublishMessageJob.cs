using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Web.Api.Services.Salaries;

namespace Web.Api.Features.BackgroundJobs.Salaries;

public class SalariesSubscriptionPublishMessageJob
    : InvocableJobBase<SalariesSubscriptionPublishMessageJob>
{
    private readonly SalariesSubscriptionService _service;

    public SalariesSubscriptionPublishMessageJob(
        ILogger<SalariesSubscriptionPublishMessageJob> logger,
        SalariesSubscriptionService service)
        : base(logger)
    {
        _service = service;
    }

    public override async Task ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString();
        var result = await _service.ProcessAllSubscriptionsAsync(
            correlationId,
            cancellationToken);

        Logger.LogInformation(
            "SalariesSubscriptionCalculateJob finished with {Count} subscriptions to process. Correlation {CorrelationId}",
            result,
            correlationId);
    }
}