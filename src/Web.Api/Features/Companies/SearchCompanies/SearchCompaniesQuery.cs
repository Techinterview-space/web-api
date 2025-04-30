using Domain.ValueObjects.Pagination;
using MediatR;
using Web.Api.Features.Companies.Dtos;

namespace Web.Api.Features.Companies.SearchCompanies;

public record SearchCompaniesQuery : SearchCompaniesQueryParams, IRequest<Pageable<CompanyDto>>
{
    public SearchCompaniesQuery(
        SearchCompaniesQueryParams queryParams)
    {
        Page = queryParams.Page;
        PageSize = queryParams.PageSize;
        SearchQuery = queryParams.SearchQuery;
    }
}