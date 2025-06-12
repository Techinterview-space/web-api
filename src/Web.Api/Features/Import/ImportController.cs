using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Enums;
using Infrastructure.Services.Mediator;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Features.Import.ImportKolesaCsv;
using Web.Api.Setup.Attributes;

namespace Web.Api.Features.Import;

[ApiController]
[Route("api/import")]
[HasAnyRole(Role.Admin)]
public class ImportController : ControllerBase
{
    private readonly IServiceProvider _serviceProvider;

    public ImportController(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    [HttpPost("kolesa-developers-csv")]
    public async Task<IActionResult> Import(
        [FromForm] ImportKolesaDevelopersCsvRequestBody request,
        CancellationToken cancellationToken)
    {
        var result = await _serviceProvider.HandleBy<ImportKolesaDevelopersCsvHandler, ImportKolesaDevelopersCsvCommand, System.Collections.Generic.List<ImportCsvResponseItem>>(
            new ImportKolesaDevelopersCsvCommand(request),
            cancellationToken);

        return Ok(result);
    }
}