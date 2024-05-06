using Domain.Entities.Questions;

namespace TechInterviewer.Features.Surveys.GetSalariesSurveyStats;

internal record SalariesSurveyStatsDbData
{
    public SurveyUsefulnessReplyType UsefulnessReply { get; init; }

    public ExpectationReplyType ExpectationReply { get; init; }
}