using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Validation;
using Domain.ValueObjects.Pagination;
using Infrastructure.Database;
using Infrastructure.Services.Mediator;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.Companies.Dtos;

namespace Web.Api.Features.Companies.SearchCompaniesForAdmin;

public class SearchCompaniesForAdminHandler
    : Infrastructure.Services.Mediator.IRequestHandler<SearchCompaniesForAdminQueryParams, Pageable<CompanyDto>>
{
    private const int MaxPageSize = 100;

    private readonly DatabaseContext _context;

    public SearchCompaniesForAdminHandler(
        DatabaseContext context)
    {
        _context = context;
    }

    public async Task<Pageable<CompanyDto>> Handle(
        SearchCompaniesForAdminQueryParams request,
        CancellationToken cancellationToken)
    {
        request.ThrowIfInvalid();

        var pageSize = request.PageSize > MaxPageSize
            ? MaxPageSize
            : request.PageSize;

        var searchQuery = request.SearchQuery?.Trim().ToLowerInvariant();
        var companies = await _context.Companies
            .AsNoTracking()
            .When(searchQuery?.Length >= 2, x => x.Name.ToLower().Contains(searchQuery))
            .OrderBy(x => x.Name)
            .AsPaginatedAsync(
                new PageModel(
                    request.Page,
                    pageSize),
                cancellationToken);

        return new Pageable<CompanyDto>(
            request.Page,
            pageSize,
            companies.TotalItems,
            companies.Results
                .Select(x => new CompanyDto(x))
                .ToList());
    }
}