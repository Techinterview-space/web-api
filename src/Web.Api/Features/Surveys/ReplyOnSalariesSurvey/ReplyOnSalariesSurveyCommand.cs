using MediatR;
using Web.Api.Features.Surveys.Dtos;

namespace Web.Api.Features.Surveys.ReplyOnSalariesSurvey;

public record ReplyOnSalariesSurveyCommand
    : ReplyOnSalariesSurveyRequestBody, IRequest<SalariesSurveyReplyDto>;