namespace Web.Api.Features.ChannelStats.GetMonthlyStatsResults;

public record GetMonthlyStatsResultsQuery
{
    public GetMonthlyStatsResultsQuery(int year, int month)
    {
        Year = year;
        Month = month;
    }

    public int Year { get; init; }

    public int Month { get; init; }
}
