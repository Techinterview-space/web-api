using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Enums;
using Domain.ValueObjects.Pagination;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.Companies.Dtos;

namespace Web.Api.Features.Companies.SearchCompanies;

public class SearchCompaniesHandler
    : IRequestHandler<SearchCompaniesQuery, Pageable<CompanyDto>>
{
    private readonly DatabaseContext _context;
    private readonly IAuthorization _authorization;

    public SearchCompaniesHandler(
        DatabaseContext context,
        IAuthorization authorization)
    {
        _context = context;
        _authorization = authorization;
    }

    public async Task<Pageable<CompanyDto>> Handle(
        SearchCompaniesQuery request,
        CancellationToken cancellationToken)
    {
        var user = await _authorization.GetCurrentUserOrNullAsync(cancellationToken);
        var userIsAdmin = user != null && user.Has(Role.Admin);

        var searchQuery = request.SearchQuery?.Trim();
        var companies = await _context.Companies
            .AsNoTracking()
            .IncludeWhen(userIsAdmin, x => x.Reviews)
            .IncludeWhen(userIsAdmin, x => x.RatingHistory)
            .When(!userIsAdmin, x => x.DeletedAt == null)
            .When(searchQuery?.Length >= 3, x => x.Name.ToLower().Contains(searchQuery))
            .OrderBy(x => x.Name)
            .AsPaginatedAsync(
                request,
                cancellationToken);

        return new Pageable<CompanyDto>(
            request.Page,
            companies.PageSize,
            companies.TotalItems,
            companies.Results
                .Select(x => new CompanyDto(x, true))
                .ToList());
    }
}