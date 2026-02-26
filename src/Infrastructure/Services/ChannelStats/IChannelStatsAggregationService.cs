using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.ChannelStats;

namespace Infrastructure.Services.ChannelStats;

public interface IChannelStatsAggregationService
{
    Task<ChannelStatsAggregationResult> RunAsync(
        StatsTriggerSource triggerSource,
        DateTimeOffset executionTimeUtc,
        CancellationToken cancellationToken = default);
}
