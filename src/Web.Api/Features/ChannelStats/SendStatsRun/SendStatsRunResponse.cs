namespace Web.Api.Features.ChannelStats.SendStatsRun;

public record SendStatsRunResponse
{
    public bool Success { get; init; }

    public string ErrorMessage { get; init; }
}
