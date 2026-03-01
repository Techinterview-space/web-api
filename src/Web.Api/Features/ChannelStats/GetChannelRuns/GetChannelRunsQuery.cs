namespace Web.Api.Features.ChannelStats.GetChannelRuns;

public record GetChannelRunsQuery
{
    public GetChannelRunsQuery(long channelId, int take)
    {
        ChannelId = channelId;
        Take = take;
    }

    public long ChannelId { get; init; }

    public int Take { get; init; }
}
