using MediatR;

namespace Web.Api.Features.Surveys.GetSalariesSurveyStats;

public record GetSalariesSurveyStatsQuery : IRequest<SalariesSurveyStatsData>;