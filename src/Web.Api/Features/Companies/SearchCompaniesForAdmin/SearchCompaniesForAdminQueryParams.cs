using Domain.ValueObjects.Pagination;

namespace Web.Api.Features.Companies.SearchCompaniesForAdmin;

public record SearchCompaniesForAdminQueryParams : PageModel
{
    public string CompanyName { get; init; } = string.Empty;

    public bool HasCompanyNameFilter()
        => !string.IsNullOrWhiteSpace(CompanyName);
}