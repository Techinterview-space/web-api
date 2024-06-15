using Domain.Entities.Questions;
using Domain.Validation.Exceptions;
using MediatR;
using Web.Api.Features.Surveys.Dtos;

namespace Web.Api.Features.Surveys.ReplyOnSalariesSurvey;

public record ReplyOnSalariesSurveyCommand
    : ReplyOnSalariesSurveyRequestBody, IRequest<SalariesSurveyReplyDto>
{
    public void IsValidOrFail()
    {
        if (ExpectationReply is ExpectationReplyType.Undefined)
        {
            throw new BadRequestException("Expectation reply is required");
        }

        if (UsefulnessReply is SurveyUsefulnessReplyType.Undefined)
        {
            throw new BadRequestException("Usefulness reply is required");
        }
    }
}