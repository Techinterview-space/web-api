namespace Web.Api.Features.PublicSurveys.SubmitPublicSurveyResponse;

public record SubmitPublicSurveyResponseCommand
{
    public SubmitPublicSurveyResponseCommand(
        string slug,
        SubmitPublicSurveyResponseRequest body)
    {
        Slug = slug;
        Body = body;
    }

    public string Slug { get; }

    public SubmitPublicSurveyResponseRequest Body { get; }
}
