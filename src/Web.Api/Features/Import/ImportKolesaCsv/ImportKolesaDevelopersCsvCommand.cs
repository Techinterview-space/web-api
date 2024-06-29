using System.Collections.Generic;
using Infrastructure.Salaries;
using MediatR;

namespace Web.Api.Features.Import.ImportKolesaCsv;

public record ImportKolesaDevelopersCsvCommand
    : ImportKolesaDevelopersCsvRequestBody, IRequest<List<UserSalaryDto>>
{
    public ImportKolesaDevelopersCsvCommand(
        ImportKolesaDevelopersCsvRequestBody body)
    {
        File = body.File;
    }
}