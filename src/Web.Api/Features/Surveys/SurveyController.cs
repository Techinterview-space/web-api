using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TechInterviewer.Features.Surveys.Dtos;
using TechInterviewer.Features.Surveys.GetSalariesSurveyStats;
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

    [HttpPost("salaries-stat-page-reply")]
    public async Task<SalariesSurveyReplyDto> ReplyOnSurveyQuestion(
        [FromBody] ReplyOnSalariesSurveyRequestBody requestBody,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new ReplyOnSalariesSurveyCommand
            {
                UsefulnessReply = requestBody.UsefulnessReply,
                ExpectationReply = requestBody.ExpectationReply,
            },
            cancellationToken);

        return result;
    }

    [HttpGet("salaries-stats")]
    public async Task<SalariesSurveyStatsData> GetSalariesSurveyStats(
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetSalariesSurveyStatsQuery(),
            cancellationToken);

        return result;
    }
}