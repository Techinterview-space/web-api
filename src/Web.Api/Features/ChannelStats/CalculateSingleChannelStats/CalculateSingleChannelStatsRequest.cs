namespace Web.Api.Features.ChannelStats.CalculateSingleChannelStats;

public record CalculateSingleChannelStatsRequest
{
    public CalculateSingleChannelStatsRequest(long channelId)
    {
        ChannelId = channelId;
    }

    public long ChannelId { get; init; }
}
