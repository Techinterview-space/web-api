using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TechInterviewer.Features.Surveys.Dtos;
using TechInterviewer.Features.Surveys.GetSalariesSurveyQuestion;
using TechInterviewer.Features.Surveys.ReplyOnSalariesSurvey;
using TechInterviewer.Setup.Attributes;

namespace TechInterviewer.Features.Surveys;

[HasAnyRole]
[ApiController]
[Route("api/survey")]
public class SurveyController
{
    private readonly IMediator _mediator;

    public SurveyController(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("salaries-survey-question")]
    public async Task<GetSalariesSurveyQuestionResponse> GetSalariesSurveyQuestion(
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetSalariesSurveyQuestionQuery(),
            cancellationToken);

        return result;
    }

    [HttpGet("{question_id:guid}/reply")]
    public async Task<SalariesSurveyReplyDto> ReplyOnSurveyQuestion(
        [FromRoute(Name = "question_id")] Guid questionId,
        [FromBody] ReplyOnSalariesSurveyRequestBody requestBody,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new ReplyOnSalariesSurveyCommand
            {
                SalariesSurveyQuestionId = questionId,
                ReplyType = requestBody.ReplyType,
            },
            cancellationToken);

        return result;
    }
}