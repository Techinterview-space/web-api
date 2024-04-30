using Domain.Entities.Questions;

namespace TechInterviewer.Features.Surveys.ReplyOnSalariesSurvey;

public record ReplyOnSalariesSurveyRequestBody
{
    public SalariesSurveyReplyType ReplyType { get; init; }
}