using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Entities.Questions;

namespace Web.Api.Features.Historical.GetSurveyHistoricalChart;

public record RecordsForPeriodCalculator
{
    public Dictionary<int, int> UsefulnessRatingData { get; }

    public List<SurveyDatabaseData> RecordsForPeriod { get; }

    public int TotalCount => RecordsForPeriod.Count;

    public RecordsForPeriodCalculator(
        List<SurveyDatabaseData> records,
        DateTimeOffset end)
    {
        RecordsForPeriod = new List<SurveyDatabaseData>();
        UsefulnessRatingData = SalariesSurveyReply.RatingValues.ToDictionary(x => x, _ => 0);

        foreach (var s in records)
        {
            if (s.CreatedAt <= end)
            {
                UsefulnessRatingData[s.UsefulnessRating]++;
                RecordsForPeriod.Add(s);
            }
            else
            {
                break;
            }
        }
    }

    public List<HistoricalSurveyReplyItem> GetUsefulnessReport()
    {
        return UsefulnessRatingData
                .Select(x => new HistoricalSurveyReplyItem(
                    x.Key,
                    GetUsefulnessPercentage(x.Key)))
                .ToList();
    }

    private double GetUsefulnessPercentage(
        int ratingValue)
    {
        return RecordsForPeriod.Count == 0
            ? 0
            : ((double)UsefulnessRatingData[ratingValue] / RecordsForPeriod.Count) * 100;
    }
}