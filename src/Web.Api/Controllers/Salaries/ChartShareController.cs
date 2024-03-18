using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Authentication.Abstract;
using Domain.Database;
using Domain.Salaries;
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
        var parameters = new SalariesChartQueryParams
        {
            Grade = request.Grade,
            ProfessionsToInclude = new DeveloperProfessionsCollection(request.ProfessionsToInclude).ToList(),
            Cities = request.Cities,
        };

        var result = await new UserChartHandler(_auth, _context).Handle(parameters, cancellationToken);
        var contentResultProvider = new ChartShareRedirectPageContentResultHandler(
            result,
            parameters,
            _context,
            _global);

        return await contentResultProvider.CreateAsync(cancellationToken);
    }
}