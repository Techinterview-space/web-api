using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.ValueObjects.Pagination;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.Companies.Dtos;

namespace Web.Api.Features.CompanyReviews.GetRecentReviews;

public class GetRecentCompanyReviewsHandler
    : Infrastructure.Services.Mediator.IRequestHandler<GetRecentCompanyReviewsQuery, Pageable<CompanyReviewDto>>
{
    private const int MaxPageSize = 100;

    private readonly DatabaseContext _context;

    public GetRecentCompanyReviewsHandler(
        DatabaseContext context)
    {
        _context = context;
    }

    public async Task<Pageable<CompanyReviewDto>> Handle(
        GetRecentCompanyReviewsQuery request,
        CancellationToken cancellationToken)
    {
        var pageSize = request.PageSize > MaxPageSize
            ? MaxPageSize
            : request.PageSize;

        var reviews = await _context.CompanyReviews
            .Include(x => x.Company)
            .Where(x =>
                x.ApprovedAt != null &&
                x.OutdatedAt == null)
            .OrderByDescending(x => x.CreatedAt)
            .AsNoTracking()
            .AsPaginatedAsync(
                new PageModel(
                    request.Page,
                    pageSize),
                cancellationToken);

        return new Pageable<CompanyReviewDto>(
            request.Page,
            pageSize,
            reviews.TotalItems,
            reviews.Results
                .Select(x => new CompanyReviewDto(x))
                .ToList());
    }
}