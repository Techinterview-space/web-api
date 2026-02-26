using System.Collections.Generic;

namespace Web.Api.Features.ChannelStats.RunMonthlyStats;

public record RunMonthlyChannelStatsResponse
{
    public List<MonthlyStatsRunDto> Runs { get; init; }

    public List<RunMonthlyChannelStatsErrorDto> Errors { get; init; }
}

public record RunMonthlyChannelStatsErrorDto
{
    public long MonitoredChannelId { get; init; }

    public string ChannelName { get; init; }

    public string ErrorMessage { get; init; }
}
