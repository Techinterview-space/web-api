using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Entities.Questions;

namespace Web.Api.Features.Historical.GetSurveyHistoricalChart;

public record RecordsForPeriodCalculator
{
    public Dictionary<SurveyUsefulnessReplyType, int> UsefulnessData { get; }

    public Dictionary<ExpectationReplyType, int> ExpectationData { get; }

    public List<SurveyDatabaseData> RecordsForPeriod { get; }

    public int TotalCount => RecordsForPeriod.Count;

    public RecordsForPeriodCalculator(
        List<SurveyDatabaseData> records,
        DateTimeOffset end)
    {
        RecordsForPeriod = new List<SurveyDatabaseData>();
        UsefulnessData = new Dictionary<SurveyUsefulnessReplyType, int>
        {
            { SurveyUsefulnessReplyType.Yes, 0 },
            { SurveyUsefulnessReplyType.No, 0 },
            { SurveyUsefulnessReplyType.NotSure, 0 },
        };

        ExpectationData = new Dictionary<ExpectationReplyType, int>
        {
            { ExpectationReplyType.Expected, 0 },
            { ExpectationReplyType.MoreThanExpected, 0 },
            { ExpectationReplyType.LessThanExpected, 0 },
        };

        foreach (var s in records)
        {
            if (s.CreatedAt <= end)
            {
                UsefulnessData[s.UsefulnessReply]++;
                ExpectationData[s.ExpectationReply]++;

                RecordsForPeriod.Add(s);
            }
            else
            {
                break;
            }
        }
    }

    public List<HistoricalSurveyReplyItem<SurveyUsefulnessReplyType>> GetUsefulnessReport()
    {
        return UsefulnessData
                .Select(x => new HistoricalSurveyReplyItem<SurveyUsefulnessReplyType>(
                    x.Key,
                    GetUsefulnessPercentage(x.Key)))
                .ToList();
    }

    public List<HistoricalSurveyReplyItem<ExpectationReplyType>> GetExpectationReport()
    {
        return ExpectationData
            .Select(x => new HistoricalSurveyReplyItem<ExpectationReplyType>(
                x.Key,
                GetExpectationPercentage(x.Key)))
            .ToList();
    }

    private double GetUsefulnessPercentage(
        SurveyUsefulnessReplyType replyType)
    {
        return RecordsForPeriod.Count == 0
            ? 0
            : ((double)UsefulnessData[replyType] / RecordsForPeriod.Count) * 100;
    }

    private double GetExpectationPercentage(
        ExpectationReplyType replyType)
    {
        return RecordsForPeriod.Count == 0
            ? 0
            : ((double)ExpectationData[replyType] / RecordsForPeriod.Count) * 100;
    }
}