using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Features.Surveys.Dtos;
using Web.Api.Features.Surveys.GetSalariesSurveyStats;
using Web.Api.Features.Surveys.GetUserSalariesSurveyData;
using Web.Api.Features.Surveys.ReplyOnSalariesSurvey;
using Web.Api.Setup.Attributes;

namespace Web.Api.Features.Surveys;

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

    [HttpGet("salaries-user-stat-data")]
    public async Task<GetUserSalariesSurveyDataResponse> GetUserSurveyStatData(
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetUserSalariesSurveyDataQuery(),
            cancellationToken);

        return result;
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