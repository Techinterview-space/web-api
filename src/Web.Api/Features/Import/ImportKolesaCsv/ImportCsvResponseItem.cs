using Infrastructure.Salaries;

namespace Web.Api.Features.Import.ImportKolesaCsv;

public record ImportCsvResponseItem
{
    public KolesaDeveloperCsvLine CsvLine { get; set; }

    public UserSalaryDto UserSalary { get; set; }
}