using MediatR;

namespace TechInterviewer.Features.Surveys.GetSalariesSurveyStats;

public record GetSalariesSurveyStatsQuery : IRequest<SalariesSurveyStatsData>;