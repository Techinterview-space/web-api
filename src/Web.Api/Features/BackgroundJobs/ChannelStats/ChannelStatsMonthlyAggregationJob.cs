using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.ChannelStats;
using Infrastructure.Services.ChannelStats;
using Microsoft.Extensions.Logging;

namespace Web.Api.Features.BackgroundJobs.ChannelStats;

public class ChannelStatsMonthlyAggregationJob : InvocableJobBase<ChannelStatsMonthlyAggregationJob>
{
    private readonly IChannelStatsAggregationService _aggregationService;

    public ChannelStatsMonthlyAggregationJob(
        ILogger<ChannelStatsMonthlyAggregationJob> logger,
        IChannelStatsAggregationService aggregationService)
        : base(logger)
    {
        _aggregationService = aggregationService;
    }

    public override async Task ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var nowUtc = DateTimeOffset.UtcNow;

        if (!IsLastDayOfMonth(nowUtc))
        {
            Logger.LogInformation(
                "ChannelStatsMonthlyAggregationJob skipped: {Date} is not the last day of the month",
                nowUtc.Date.ToString("yyyy-MM-dd"));
            return;
        }

        Logger.LogInformation(
            "ChannelStatsMonthlyAggregationJob starting: {Date} is the last day of the month",
            nowUtc.Date.ToString("yyyy-MM-dd"));

        var result = await _aggregationService.RunAsync(
            StatsTriggerSource.Scheduled,
            nowUtc,
            cancellationToken);

        Logger.LogInformation(
            "ChannelStatsMonthlyAggregationJob completed: {RunCount} channels processed, {ErrorCount} errors",
            result.Runs.Count,
            result.Errors.Count);
    }

    private static bool IsLastDayOfMonth(DateTimeOffset dateUtc)
    {
        return dateUtc.Day == DateTime.DaysInMonth(dateUtc.Year, dateUtc.Month);
    }
}
