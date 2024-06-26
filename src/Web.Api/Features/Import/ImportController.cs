using System.Threading;
using System.Threading.Tasks;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Features.Import.ImportKolesaCsv;
using Web.Api.Setup.Attributes;

namespace Web.Api.Features.Import;

[ApiController]
[Route("api/import")]
[HasAnyRole(Role.Admin)]
public class ImportController : ControllerBase
{
    private readonly IMediator _mediator;

    public ImportController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("kolesa-developers-csv")]
    public async Task<IActionResult> Import(
        [FromForm] ImportKolesaDevelopersCsvRequestBody request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new ImportKolesaDevelopersCsvCommand(request),
            cancellationToken);

        return Ok(result);
    }
}