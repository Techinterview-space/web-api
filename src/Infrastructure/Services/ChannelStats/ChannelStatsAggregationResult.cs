using System.Collections.Generic;
using Domain.Entities.ChannelStats;

namespace Infrastructure.Services.ChannelStats;

public record ChannelStatsAggregationResult
{
    public ChannelStatsAggregationResult(
        List<MonthlyStatsRun> runs,
        List<ChannelStatsAggregationError> errors)
    {
        Runs = runs;
        Errors = errors;
    }

    public List<MonthlyStatsRun> Runs { get; init; }

    public List<ChannelStatsAggregationError> Errors { get; init; }
}

public record ChannelStatsAggregationError
{
    public ChannelStatsAggregationError(
        long monitoredChannelId,
        string channelName,
        string errorMessage)
    {
        MonitoredChannelId = monitoredChannelId;
        ChannelName = channelName;
        ErrorMessage = errorMessage;
    }

    public long MonitoredChannelId { get; init; }

    public string ChannelName { get; init; }

    public string ErrorMessage { get; init; }
}
