namespace Web.Api.Features.Surveys.GetSalariesSurveyStats;

public record SalariesSurveyStatsDataItem
{
    public SalariesSurveyStatsDataItem(
        int countOfReplies,
        int totalCountOfReplies)
    {
        CountOfReplies = countOfReplies;
        PartitionInPercent = (double)countOfReplies / totalCountOfReplies * 100;
    }

    public int CountOfReplies { get; }

    public double PartitionInPercent { get; }
}