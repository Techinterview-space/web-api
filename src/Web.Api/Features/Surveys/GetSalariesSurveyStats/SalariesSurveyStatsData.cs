using System.Collections.Generic;

namespace Web.Api.Features.Surveys.GetSalariesSurveyStats;

public record SalariesSurveyStatsData
{
    public int CountOfRecords { get; init; }

    public List<ReplyDataItem> UsefulnessData { get; init; }

    public record ReplyDataItem
    {
        public ReplyDataItem(
            int ratingValue,
            SalariesSurveyStatsDataItem data)
        {
            RatingValue = ratingValue;
            Data = data;
        }

        public int RatingValue { get; }

        public SalariesSurveyStatsDataItem Data { get; }
    }
}