using System.Collections.Generic;

namespace Web.Api.Features.Import.ImportKolesaCsv;

public record ImportKolesaDevelopersCsvCommand
    : ImportKolesaDevelopersCsvRequestBody, IRequest<List<ImportCsvResponseItem>>
{
    public ImportKolesaDevelopersCsvCommand(
        ImportKolesaDevelopersCsvRequestBody body)
    {
        File = body.File;
    }
}