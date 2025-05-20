using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Enums;
using Domain.ValueObjects.Pagination;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using Infrastructure.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.Companies.Dtos;

namespace Web.Api.Features.Companies.SearchCompanies;

public class SearchCompaniesHandler
    : IRequestHandler<SearchCompaniesQuery, SearchCompaniesResponse>
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

    public async Task<SearchCompaniesResponse> Handle(
        SearchCompaniesQuery request,
        CancellationToken cancellationToken)
    {
        var pageSize = request.GetPageSize();

        var user = await _authorization.GetCurrentUserOrNullAsync(cancellationToken);
        var userHasAnyReview =
            user is not null &&
            await _context.CompanyReviews
                .HasRecentReviewAsync(
                    companyId: null,
                    userId: user.Id,
                    countOfMonthsForEdge: 12,
                    cancellationToken: cancellationToken);

        var searchQuery = request.SearchQuery?.Trim().ToLowerInvariant();
        var companies = await _context.Companies
            .AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .When(searchQuery?.Length >= 2, x => x.Name.ToLower().Contains(searchQuery))
            .When(request.WithRating && !request.HasSearchQuery(), x => x.Rating > 0)
            .OrderByDescending(x => x.ReviewsCount)
            .ThenByDescending(x => x.ViewsCount)
            .ThenByDescending(x => x.Name)
            .AsPaginatedAsync(
                new PageModel(
                    request.Page,
                    pageSize),
                cancellationToken);

        return new SearchCompaniesResponse(
            request.Page,
            pageSize,
            companies.TotalItems,
            companies.Results
                .Select(x => new CompanyDto(x))
                .ToList(),
            userHasAnyReview);
    }
}