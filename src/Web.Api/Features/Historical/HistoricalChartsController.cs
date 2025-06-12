using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Salaries;
using Infrastructure.Services.Mediator;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Features.Historical.GetSalariesHistoricalChart;
using Web.Api.Features.Historical.GetSurveyHistoricalChart;

namespace Web.Api.Features.Historical;

[ApiController]
[Route("api/historical-charts")]
public class HistoricalChartsController : ControllerBase
{
    private readonly IServiceProvider _serviceProvider;

    public HistoricalChartsController(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    [HttpGet("salaries")]
    public Task<GetSalariesHistoricalChartResponse> GetSalariesHistoricalChart(
        [FromQuery] GetSalariesHistoricalChartQueryParams request,
        CancellationToken cancellationToken)
    {
        return _serviceProvider.HandleBy<GetSalariesHistoricalChartHandler, GetSalariesHistoricalChartQueryParams, GetSalariesHistoricalChartResponse>(
            new GetSalariesHistoricalChartQueryParams
            {
                From = request.From,
                To = request.To,
                Grade = request.Grade,
                SelectedProfessionIds = new DeveloperProfessionsCollection(request.SelectedProfessionIds).ToList(),
                Cities = request.Cities,
                Skills = request.Skills,
                SalarySourceTypes = request.SalarySourceTypes,
                QuarterTo = request.QuarterTo,
                YearTo = request.YearTo,
            },
            cancellationToken);
    }

    [HttpGet("survey")]
    public Task<GetSurveyHistoricalChartResponse> GetSurveyHistoricalChart(
        [FromQuery] GetSalariesHistoricalChartQueryParams request,
        CancellationToken cancellationToken)
    {
        return _serviceProvider.HandleBy<GetSurveyHistoricalChartHandler, GetSurveyHistoricalChartQuery, GetSurveyHistoricalChartResponse>(
            new GetSurveyHistoricalChartQuery
            {
                From = request.From,
                To = request.To,
                Grade = request.Grade,
                SelectedProfessionIds = new DeveloperProfessionsCollection(request.SelectedProfessionIds).ToList(),
                Cities = request.Cities,
                Skills = request.Skills,
                SalarySourceTypes = request.SalarySourceTypes,
                QuarterTo = request.QuarterTo,
                YearTo = request.YearTo,
            },
            cancellationToken);
    }
}