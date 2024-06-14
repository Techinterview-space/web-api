using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Salaries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TechInterviewer.Features.Salaries.GetSalariesHistoricalChart;

namespace TechInterviewer.Features.Salaries;

[ApiController]
[Route("api/historical-charts")]
public class HistoricalChartsController : ControllerBase
{
    private readonly IMediator _mediator;

    public HistoricalChartsController(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("salaries")]
    public Task<GetSalariesHistoricalChartResponse> GetHistoricalChart(
        [FromQuery] GetSalariesHistoricalChartQueryParams request,
        CancellationToken cancellationToken)
    {
        return _mediator.Send(
            new GetSalariesHistoricalChartQuery
            {
                From = request.From,
                To = request.To,
                Grade = request.Grade,
                ProfessionsToInclude = new DeveloperProfessionsCollection(request.ProfessionsToInclude).ToList(),
                Cities = request.Cities,
            },
            cancellationToken);
    }
}