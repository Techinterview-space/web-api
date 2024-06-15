using MediatR;

namespace Web.Api.Features.Surveys.GetUserSalariesSurveyData;

public record GetUserSalariesSurveyDataQuery : IRequest<GetUserSalariesSurveyDataResponse>;