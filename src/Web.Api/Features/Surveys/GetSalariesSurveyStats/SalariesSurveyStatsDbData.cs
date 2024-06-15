using Domain.Entities.Questions;

namespace Web.Api.Features.Surveys.GetSalariesSurveyStats;

internal record SalariesSurveyStatsDbData
{
    public SurveyUsefulnessReplyType UsefulnessReply { get; init; }

    public ExpectationReplyType ExpectationReply { get; init; }
}