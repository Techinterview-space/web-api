using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Web.Api.Services.Salaries;

namespace Web.Api.Features.BackgroundJobs;

public class StatDataChangeSubscriptionCalculateJob
    : InvocableJobBase<StatDataChangeSubscriptionCalculateJob>
{
    private readonly StatDataChangeSubscriptionService _service;

    public StatDataChangeSubscriptionCalculateJob(
        ILogger<StatDataChangeSubscriptionCalculateJob> logger,
        StatDataChangeSubscriptionService service)
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
            "StatDataChangeSubscriptionCalculateJob finished with {Count} subscriptions to process. Correlation {CorrelationId}",
            result,
            correlationId);
    }
}