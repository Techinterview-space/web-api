using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Web.Api.Services.Salaries;

namespace Web.Api.Features.BackgroundJobs;

public class SalariesSubscriptionCalculateJob
    : InvocableJobBase<SalariesSubscriptionCalculateJob>
{
    private readonly SalariesSubscriptionService _service;

    public SalariesSubscriptionCalculateJob(
        ILogger<SalariesSubscriptionCalculateJob> logger,
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