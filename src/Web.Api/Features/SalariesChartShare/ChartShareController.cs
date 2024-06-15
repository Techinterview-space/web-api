using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Features.SalariesChartShare.GetChartSharePage;

namespace Web.Api.Features.SalariesChartShare;

[ApiController]
[Route("chart-share")]
public class ChartShareController : ControllerBase
{
    private readonly IMediator _mediator;

    public ChartShareController(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("")]
    public async Task<IActionResult> ChartShareAsync(
        [FromQuery] GetChartSharePageQuery request,
        CancellationToken cancellationToken)
    {
        var contentHtml = await _mediator.Send(
            request,
            cancellationToken);

        return new ContentResult
        {
            Content = contentHtml,
            ContentType = "text/html",
            StatusCode = 200,
        };
    }
}