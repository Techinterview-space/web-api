using Domain.ValueObjects.Pagination;

namespace Web.Api.Features.Companies.SearchCompanies;

public record SearchCompaniesQueryParams : PageModel
{
    public string SearchQuery { get; init; } = string.Empty;
}