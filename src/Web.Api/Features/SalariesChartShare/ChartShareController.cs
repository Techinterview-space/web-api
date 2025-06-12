using System;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Services.Mediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Web.Api.Features.SalariesChartShare.GetChartSharePage;

namespace Web.Api.Features.SalariesChartShare;

[ApiController]
[Route("chart-share")]
public class ChartShareController : ControllerBase
{
    private readonly IServiceProvider _serviceProvider;

    public ChartShareController(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    [HttpGet("")]
    public async Task<IActionResult> ChartShareAsync(
        [FromQuery] GetChartSharePageQuery request,
        CancellationToken cancellationToken)
    {
        var contentHtml = await _serviceProvider.HandleBy<GetChartSharePageHandler, GetChartSharePageQuery, string>(
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