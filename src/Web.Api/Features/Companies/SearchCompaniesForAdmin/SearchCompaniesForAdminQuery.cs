using Domain.ValueObjects.Pagination;
using MediatR;
using Web.Api.Features.Companies.Dtos;

namespace Web.Api.Features.Companies.SearchCompaniesForAdmin;

public record SearchCompaniesForAdminQuery : SearchCompaniesForAdminQueryParams, IRequest<Pageable<CompanyDto>>
{
    public SearchCompaniesForAdminQuery(
        SearchCompaniesForAdminQueryParams queryParams)
    {
        Page = queryParams.Page;
        PageSize = queryParams.PageSize;
        SearchQuery = queryParams.SearchQuery;
    }
}