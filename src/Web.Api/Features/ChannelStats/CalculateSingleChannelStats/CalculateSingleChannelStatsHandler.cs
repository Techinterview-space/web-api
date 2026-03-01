using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.ChannelStats;
using Infrastructure.Services.ChannelStats;
using Web.Api.Features.ChannelStats.RunMonthlyStats;

namespace Web.Api.Features.ChannelStats.CalculateSingleChannelStats;

public class CalculateSingleChannelStatsHandler
    : Infrastructure.Services.Mediator.IRequestHandler<CalculateSingleChannelStatsRequest, RunMonthlyChannelStatsResponse>
{
    private readonly IChannelStatsAggregationService _aggregationService;

    public CalculateSingleChannelStatsHandler(
        IChannelStatsAggregationService aggregationService)
    {
        _aggregationService = aggregationService;
    }

    public async Task<RunMonthlyChannelStatsResponse> Handle(
        CalculateSingleChannelStatsRequest request,
        CancellationToken cancellationToken)
    {
        var run = await _aggregationService.RunForChannelAsync(
            request.ChannelId,
            StatsTriggerSource.Manual,
            DateTimeOffset.UtcNow,
            cancellationToken);

        return new RunMonthlyChannelStatsResponse
        {
            Runs = new () { new MonthlyStatsRunDto(run) },
            Errors = new (),
        };
    }
}
