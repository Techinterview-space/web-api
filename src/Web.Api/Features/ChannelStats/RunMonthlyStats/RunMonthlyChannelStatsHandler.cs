using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.ChannelStats;
using Infrastructure.Services.ChannelStats;

namespace Web.Api.Features.ChannelStats.RunMonthlyStats;

public class RunMonthlyChannelStatsHandler
    : Infrastructure.Services.Mediator.IRequestHandler<RunMonthlyChannelStatsRequest, RunMonthlyChannelStatsResponse>
{
    private readonly IChannelStatsAggregationService _aggregationService;

    public RunMonthlyChannelStatsHandler(
        IChannelStatsAggregationService aggregationService)
    {
        _aggregationService = aggregationService;
    }

    public async Task<RunMonthlyChannelStatsResponse> Handle(
        RunMonthlyChannelStatsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _aggregationService.RunAsync(
            StatsTriggerSource.Manual,
            DateTimeOffset.UtcNow,
            cancellationToken);

        return new RunMonthlyChannelStatsResponse
        {
            Runs = result.Runs
                .Select(x => new MonthlyStatsRunDto(x))
                .ToList(),
            Errors = result.Errors
                .Select(x => new RunMonthlyChannelStatsErrorDto
                {
                    MonitoredChannelId = x.MonitoredChannelId,
                    ChannelName = x.ChannelName,
                    ErrorMessage = x.ErrorMessage,
                })
                .ToList(),
        };
    }
}
