namespace Web.Api.Features.Surveys.ReplyOnSalariesSurvey;

public record ReplyOnSalariesSurveyRequestBody
{
    public int UsefulnessRating { get; init; }
}