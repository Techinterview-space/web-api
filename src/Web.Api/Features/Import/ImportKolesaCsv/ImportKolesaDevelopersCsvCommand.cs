using System.Collections.Generic;
using MediatR;

namespace Web.Api.Features.Import.ImportKolesaCsv;

public record ImportKolesaDevelopersCsvCommand
    : ImportKolesaDevelopersCsvRequestBody, IRequest<List<KolesaDeveloperCsvLine>>
{
    public ImportKolesaDevelopersCsvCommand(
        ImportKolesaDevelopersCsvRequestBody body)
    {
        File = body.File;
    }
}