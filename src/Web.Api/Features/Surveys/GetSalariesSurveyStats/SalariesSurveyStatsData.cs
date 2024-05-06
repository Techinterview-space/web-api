using System.Collections.Generic;
using Domain.Entities.Questions;

namespace TechInterviewer.Features.Surveys.GetSalariesSurveyStats;

public record SalariesSurveyStatsData
{
    public int CountOfRecords { get; init; }

    public Dictionary<SurveyUsefulnessReplyType, SalariesSurveyStatsDataItem> UsefulnessData { get; init; }

    public Dictionary<ExpectationReplyType, SalariesSurveyStatsDataItem> ExpectationData { get; init; }
}