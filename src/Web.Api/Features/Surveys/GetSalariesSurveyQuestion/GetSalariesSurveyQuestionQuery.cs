using MediatR;
using TechInterviewer.Features.Surveys.Dtos;

namespace TechInterviewer.Features.Surveys.GetSalariesSurveyQuestion;

public record GetSalariesSurveyQuestionQuery : IRequest<GetSalariesSurveyQuestionResponse>;