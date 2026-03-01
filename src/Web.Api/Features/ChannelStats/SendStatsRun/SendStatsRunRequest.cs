namespace Web.Api.Features.ChannelStats.SendStatsRun;

public record SendStatsRunRequest
{
    public SendStatsRunRequest(long runId)
    {
        RunId = runId;
    }

    public long RunId { get; init; }
}
