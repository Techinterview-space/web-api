using Domain.Entities.Questions;

namespace TechInterviewer.Features.Surveys.ReplyOnSalariesSurvey;

public record ReplyOnSalariesSurveyRequestBody
{
    public SurveyUsefulnessReplyType UsefulnessReply { get; init; }

    public ExpectationReplyType ExpectationReply { get; init; }
}