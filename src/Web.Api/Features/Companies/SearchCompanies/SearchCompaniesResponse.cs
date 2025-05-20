using System.Collections.Generic;
using Domain.ValueObjects.Pagination;
using Web.Api.Features.Companies.Dtos;

namespace Web.Api.Features.Companies.SearchCompanies;

public record SearchCompaniesResponse : Pageable<CompanyDto>
{
    public bool UserHasAnyReview { get; init; }

    public SearchCompaniesResponse(
        int currentPage,
        int pageSize,
        int totalItems,
        IReadOnlyCollection<CompanyDto> results,
        bool userHasAnyReview)
        : base(
            currentPage,
            pageSize,
            totalItems,
            results)
    {
        UserHasAnyReview = userHasAnyReview;
    }
}