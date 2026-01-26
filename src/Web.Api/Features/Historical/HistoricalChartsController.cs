using System;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Services.Mediator;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Features.Historical.GetSalariesHistoricalChart;

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
            },
            cancellationToken);
    }
}