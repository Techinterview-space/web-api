using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Enums;
using Infrastructure.Services.Mediator;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Features.ChannelStats.CalculateSingleChannelStats;
using Web.Api.Features.ChannelStats.Channels;
using Web.Api.Features.ChannelStats.GetChannelRuns;
using Web.Api.Features.ChannelStats.GetMonthlyStatsResults;
using Web.Api.Features.ChannelStats.RunMonthlyStats;
using Web.Api.Features.ChannelStats.SendStatsRun;
using Web.Api.Setup.Attributes;

namespace Web.Api.Features.ChannelStats;

[ApiController]
[Route("api/admin/channel-stats")]
[HasAnyRole(Role.Admin)]
public class ChannelStatsAdminController : ControllerBase
{
    private readonly IServiceProvider _serviceProvider;

    public ChannelStatsAdminController(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    [HttpGet("channels")]
    public async Task<List<MonitoredChannelDto>> GetChannels(
        CancellationToken cancellationToken)
    {
        return await _serviceProvider
            .HandleBy<GetMonitoredChannelsHandler, GetMonitoredChannelsQuery, List<MonitoredChannelDto>>(
                new GetMonitoredChannelsQuery(),
                cancellationToken);
    }

    [HttpPost("channels")]
    public async Task<MonitoredChannelDto> CreateChannel(
        [FromBody] CreateMonitoredChannelRequest request,
        CancellationToken cancellationToken)
    {
        return await _serviceProvider
            .HandleBy<CreateMonitoredChannelHandler, CreateMonitoredChannelRequest, MonitoredChannelDto>(
                request,
                cancellationToken);
    }

    [HttpPut("channels/{id:long}")]
    public async Task<MonitoredChannelDto> UpdateChannel(
        [FromRoute] long id,
        [FromBody] UpdateMonitoredChannelRequest request,
        CancellationToken cancellationToken)
    {
        return await _serviceProvider
            .HandleBy<UpdateMonitoredChannelHandler, UpdateMonitoredChannelCommand, MonitoredChannelDto>(
                new UpdateMonitoredChannelCommand(id, request),
                cancellationToken);
    }

    [HttpPost("run")]
    public async Task<RunMonthlyChannelStatsResponse> RunMonthlyStats(
        CancellationToken cancellationToken)
    {
        return await _serviceProvider
            .HandleBy<RunMonthlyChannelStatsHandler, RunMonthlyChannelStatsRequest, RunMonthlyChannelStatsResponse>(
                new RunMonthlyChannelStatsRequest(),
                cancellationToken);
    }

    [HttpGet("results")]
    public async Task<List<MonthlyStatsRunDto>> GetResults(
        [FromQuery] int year,
        [FromQuery] int month,
        CancellationToken cancellationToken)
    {
        return await _serviceProvider
            .HandleBy<GetMonthlyStatsResultsHandler, GetMonthlyStatsResultsQuery, List<MonthlyStatsRunDto>>(
                new GetMonthlyStatsResultsQuery(year, month),
                cancellationToken);
    }

    [HttpPost("channels/{channelId:long}/calculate")]
    public async Task<RunMonthlyChannelStatsResponse> CalculateChannelStats(
        [FromRoute] long channelId,
        CancellationToken cancellationToken)
    {
        return await _serviceProvider
            .HandleBy<CalculateSingleChannelStatsHandler, CalculateSingleChannelStatsRequest, RunMonthlyChannelStatsResponse>(
                new CalculateSingleChannelStatsRequest(channelId),
                cancellationToken);
    }

    [HttpGet("channels/{channelId:long}/runs")]
    public async Task<List<MonthlyStatsRunDto>> GetChannelRuns(
        [FromRoute] long channelId,
        [FromQuery] int take = 3,
        CancellationToken cancellationToken = default)
    {
        return await _serviceProvider
            .HandleBy<GetChannelRunsHandler, GetChannelRunsQuery, List<MonthlyStatsRunDto>>(
                new GetChannelRunsQuery(channelId, take),
                cancellationToken);
    }

    [HttpPost("runs/{runId:long}/send")]
    public async Task<SendStatsRunResponse> SendStatsRun(
        [FromRoute] long runId,
        CancellationToken cancellationToken)
    {
        return await _serviceProvider
            .HandleBy<SendStatsRunHandler, SendStatsRunRequest, SendStatsRunResponse>(
                new SendStatsRunRequest(runId),
                cancellationToken);
    }
}
