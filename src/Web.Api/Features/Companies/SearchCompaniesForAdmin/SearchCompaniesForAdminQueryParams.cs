using Domain.ValueObjects.Pagination;

namespace Web.Api.Features.Companies.SearchCompaniesForAdmin;

public record SearchCompaniesForAdminQueryParams : PageModel
{
    public string SearchQuery { get; init; } = string.Empty;

    public string CompanyName { get; init; } = string.Empty;

    public bool HasSearchQuery()
        => !string.IsNullOrWhiteSpace(SearchQuery);

    public bool HasCompanyNameFilter()
        => !string.IsNullOrWhiteSpace(CompanyName);
}