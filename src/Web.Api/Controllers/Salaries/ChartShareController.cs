using System.Threading;
using System.Threading.Tasks;
using Domain.Authentication.Abstract;
using Domain.Database;
using Domain.Services.Global;
using Microsoft.AspNetCore.Mvc;
using TechInterviewer.Controllers.Salaries.Charts;
using TechInterviewer.Features.Charts;

namespace TechInterviewer.Controllers.Salaries;

[ApiController]
[Route("chart-share")]
public class ChartShareController : ControllerBase
{
    private readonly DatabaseContext _context;
    private readonly IGlobal _global;
    private readonly IAuthorization _auth;

    public ChartShareController(
        DatabaseContext context,
        IGlobal global,
        IAuthorization auth)
    {
        _context = context;
        _global = global;
        _auth = auth;
    }

    [HttpGet("")]
    public async Task<IActionResult> ChartShareAsync(
        [FromQuery] SalariesChartQueryParams request,
        CancellationToken cancellationToken)
    {
        var result = await new UserChartHandler(_auth, _context).Handle(request, cancellationToken);
        var contentResultProvider = new ChartShareRedirectPageContentResultHandler(
            result,
            request,
            _context,
            _global);

        return await contentResultProvider.CreateAsync(cancellationToken);
    }
}