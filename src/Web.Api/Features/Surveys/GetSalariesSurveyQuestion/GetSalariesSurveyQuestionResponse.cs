using TechInterviewer.Features.Surveys.Dtos;

namespace TechInterviewer.Features.Surveys.GetSalariesSurveyQuestion;

public record GetSalariesSurveyQuestionResponse
{
    public SalariesSurveyQuestionDto Question { get; }

    public bool RequiresReply { get; }

    public GetSalariesSurveyQuestionResponse(
        SalariesSurveyQuestionDto question,
        bool requiresReply)
    {
        Question = question;
        RequiresReply = requiresReply;
    }
}