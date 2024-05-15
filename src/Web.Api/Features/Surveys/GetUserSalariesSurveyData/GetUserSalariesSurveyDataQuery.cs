using MediatR;

namespace TechInterviewer.Features.Surveys.GetUserSalariesSurveyData;

public record GetUserSalariesSurveyDataQuery : IRequest<GetUserSalariesSurveyDataResponse>;