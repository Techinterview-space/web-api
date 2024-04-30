using System;
using Domain.Entities.Questions;
using Domain.Validation.Exceptions;
using MediatR;
using TechInterviewer.Features.Surveys.Dtos;

namespace TechInterviewer.Features.Surveys.ReplyOnSalariesSurvey;

public record ReplyOnSalariesSurveyCommand
    : ReplyOnSalariesSurveyRequestBody, IRequest<SalariesSurveyReplyDto>
{
    public Guid SalariesSurveyQuestionId { get; init; }

    public void IsValidOrFail()
    {
        if (ReplyType is SalariesSurveyReplyType.Undefined)
        {
            throw new BadRequestException("Reply type is required");
        }

        if (SalariesSurveyQuestionId == Guid.Empty)
        {
            throw new BadRequestException("Salaries survey question id is required");
        }
    }
}