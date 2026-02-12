using Domain.Entities.Surveys;

namespace TestUtils.Fakes;

public class PublicSurveyQuestionFake : PublicSurveyQuestion
{
    public PublicSurveyQuestionFake(
        PublicSurvey survey,
        string text = null,
        int order = 0,
        bool allowMultipleChoices = false)
        : base(
            text ?? "Test question?",
            order,
            survey,
            allowMultipleChoices)
    {
    }
}
