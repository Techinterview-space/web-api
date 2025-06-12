using System;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Services.Mediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Web.Api.Features.Surveys.Dtos;
using Web.Api.Features.Surveys.GetSalariesSurveyStats;
using Web.Api.Features.Surveys.GetUserSalariesSurveyData;
using Web.Api.Features.Surveys.ReplyOnSalariesSurvey;
using Web.Api.Setup.Attributes;

namespace Web.Api.Features.Surveys;

[HasAnyRole]
[ApiController]
[Route("api/survey")]
public class SurveyController : Controller
{
    private readonly IServiceProvider _serviceProvider;

    public SurveyController(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    [HttpGet("salaries-user-stat-data")]
    public async Task<GetUserSalariesSurveyDataResponse> GetUserSurveyStatData(
        CancellationToken cancellationToken)
    {
        return await _serviceProvider.HandleBy<GetUserSalariesSurveyDataHandler, GetUserSalariesSurveyDataQuery, GetUserSalariesSurveyDataResponse>(
            new GetUserSalariesSurveyDataQuery(),
            cancellationToken);
    }

    [HttpPost("salaries-stat-page-reply")]
    public async Task<SalariesSurveyReplyDto> ReplyOnSurveyQuestion(
        [FromBody] ReplyOnSalariesSurveyRequestBody requestBody,
        CancellationToken cancellationToken)
    {
        return await _serviceProvider.HandleBy<ReplyOnSalariesSurveyHandler, ReplyOnSalariesSurveyCommand, SalariesSurveyReplyDto>(
            new ReplyOnSalariesSurveyCommand
            {
                UsefulnessRating = requestBody.UsefulnessRating,
            },
            cancellationToken);
    }

    [HttpGet("salaries-stats")]
    public async Task<SalariesSurveyStatsData> GetSalariesSurveyStats(
        CancellationToken cancellationToken)
    {
        return await _serviceProvider.HandleBy<GetSalariesSurveyStatsHandler, GetSalariesSurveyStatsQuery, SalariesSurveyStatsData>(
            new GetSalariesSurveyStatsQuery(),
            cancellationToken);
    }
}