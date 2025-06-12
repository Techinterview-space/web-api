using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Database;
using Infrastructure.Services.Mediator;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.Companies.Dtos;

namespace Web.Api.Features.CompanyReviews.SearchReviewsToBeApproved;

public class SearchReviewsToBeApprovedHandler
    : Infrastructure.Services.Mediator.IRequestHandler<SearchReviewsToBeApprovedQuery, List<CompanyReviewDto>>
{
    private readonly DatabaseContext _context;

    public SearchReviewsToBeApprovedHandler(
        DatabaseContext context)
    {
        _context = context;
    }

    public async Task<List<CompanyReviewDto>> Handle(
        SearchReviewsToBeApprovedQuery request,
        CancellationToken cancellationToken)
    {
        var reviews = await _context.CompanyReviews
            .AsNoTracking()
            .Include(x => x.Company)
            .Where(x =>
                x.ApprovedAt == null &&
                x.OutdatedAt == null)
            .Select(CompanyReviewDto.Transformation)
            .ToListAsync(cancellationToken);

        return reviews;
    }
}