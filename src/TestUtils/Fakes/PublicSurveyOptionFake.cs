using Domain.Entities.Surveys;

namespace TestUtils.Fakes;

public class PublicSurveyOptionFake : PublicSurveyOption
{
    public PublicSurveyOptionFake(
        PublicSurveyQuestion question,
        string text = null,
        int order = 0)
        : base(
            text ?? "Test option",
            order,
            question)
    {
    }
}
