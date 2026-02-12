using Domain.Enums;

namespace Web.Api.Features.PublicSurveys.GetMyPublicSurveys;

public record GetMyPublicSurveysQuery
{
    public bool IncludeDeleted { get; init; }

    public PublicSurveyStatus? Status { get; init; }
}
